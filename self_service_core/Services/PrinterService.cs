using System.Net;
using System.Text;
using ESCPOS_NET;
using ESCPOS_NET.Emitters;
using ESCPOS_NET.Utilities;
using self_service_core.Models;

namespace self_service_core.Services;

public class PrinterService : IPrinterService
{

    private IEnumerable<NetworkPrinter>? _connectedPrinters;
    private IEnumerable<NetworkPrinter>? _selectedPrinters;
    private readonly ICommandEmitter _e = new EPSON();
    readonly Encoding _encoding = Encoding.UTF8;
    private readonly IMongoDbService _mongoDbService;

    public PrinterService(IMongoDbService mongoDbService)
    {
        _mongoDbService = mongoDbService;
        _connectedPrinters = new List<NetworkPrinter>();
    }
    
    public async Task<IEnumerable<PrinterModel>> GetPrinters(OrderModel order)
    {
        IEnumerable<PrinterModel> printers = await _mongoDbService.GetPrinters(null);
        printers = printers.Where(printer => printer.isDefault ?? false).ToList();
        return printers;
    }
    
    public async Task<IEnumerable<PrinterModel>> GetPrinters(OrderItemModel orderItem)
    {
        IEnumerable<PrinterModel> printers = await _mongoDbService.GetPrinters(orderItem.CategoryId);
        return printers;
    }
    
    
    public async Task<Task> VerifyPrinters(IEnumerable<PrinterModel> printers)
    {
        _selectedPrinters = new List<NetworkPrinter>();
        foreach (var printer in printers)
        {
            if (!_connectedPrinters.Any(p => p.PrinterName == printer.Name))
            {
                await SetPrinter(printer);
            }

            _selectedPrinters = _selectedPrinters.Append(_connectedPrinters.First(p => p.PrinterName == printer.Name));
        }
        return Task.CompletedTask;
    }
    
    public async Task SendToPrinters(OrderModel order)
    {
        IEnumerable<PrinterModel> printers = await GetPrinters(order);
        await VerifyPrinters(printers);
        foreach (var printer in _selectedPrinters)
        {
            await Print(order, printer);
        }
        
    }
    
    public async Task SendToPrinters(OrderItemModel orderItem)
    {
        IEnumerable<PrinterModel> printers = await GetPrinters(orderItem);
        await VerifyPrinters(printers);
        foreach (var printer in _selectedPrinters)
        {
            await Print(orderItem, printer);
        }
        
    }
    
    
    
    public async Task<NetworkPrinter> SetPrinter(PrinterModel printer)
    {
        NetworkPrinter _printer = new NetworkPrinter(new NetworkPrinterSettings() { ConnectionString = $"{printer.Host}:{printer.Port}", PrinterName = printer.Name });
        _printer.Connected += (sender, args) => Console.WriteLine("Connected to printer");
        _printer.Disconnected += (sender, args) => Console.WriteLine("Disconnected from printer");
        _printer.StatusChanged += StatusChanged!;

        if (_connectedPrinters != null)
        {
            _connectedPrinters = _connectedPrinters.Append(_printer);
        }

        return _printer;
    }


    public Task Print(OrderItemModel orderItem, NetworkPrinter printer)
    {
        //PT-BR
        
        var print = ByteSplicer.Combine([
                _e.CenterAlign(),
                _e.PrintLine("--------------------------------------------------"),
                _e.PrintLine("Pedido "+ orderItem.ItemId),
                _e.PrintLine("Data e Hora: "+ orderItem.CreatedAt.ToString("dd/MM/yyyy HH:mm")),
                _e.PrintLine("--------------------------------------------------"),
                _e.PrintLine("Mesa: "+ orderItem.CardNumber),
                _e.PrintLine("--------------------------------------------------"),
                _e.LeftAlign(),
                _encoding.GetBytes("Item: "+ orderItem.Name),
                _e.PrintLine(""),
                _e.PrintLine("Quantidade: "+ orderItem.Quantity),
                _encoding.GetBytes("Preço: R$"+ (orderItem.IsPromotion ?? false ? orderItem.PromotionPrice?.ToString("F2") : orderItem.Price?.ToString("F2"))),
                _e.PrintLine(""),
                _e.PrintLine(""),
                _encoding.GetBytes("Adicionais: "+ (orderItem.Additionals.Count > 0 ? string.Join(", ", orderItem.Additionals.Select(additional => additional.Name)) : "Nenhum adicional selecionado")),
                _e.PrintLine(""),
                _encoding.GetBytes("Observação: "+ (string.IsNullOrEmpty(orderItem.Observation) ? "Nenhuma observação" : orderItem.Observation)),
                _e.PrintLine(""),
                _e.CenterAlign(),
                _e.PrintLine("--------------------------------------------------"),
                _e.LeftAlign(),
                _e.PrintLine("SubTotal: R$"+ orderItem.Total?.ToString("F2")),
                _e.CenterAlign(),
                _e.PrintLine("--------------------------------------------------"),
            ]
        );

        SendToPrinter(print, printer);
        return Task.CompletedTask;
    }

    public Task Print(OrderModel order, NetworkPrinter printer)
    {
        
        var print = ByteSplicer.Combine([
                _e.CenterAlign(),
                _e.PrintLine("--------------------------------------------------"),
                _e.PrintLine("Comanda "+ order.OrderId),
                _e.PrintLine("Data e Hora: "+ order.CreatedAt.ToString("dd/MM/yyyy HH:mm")),
                _e.PrintLine("--------------------------------------------------"),
                _e.PrintLine("Mesa: "+ order.CardNumber),
                _e.PrintLine("--------------------------------------------------"),
                _e.LeftAlign(),
                ..GetPrintItems(order.Items),
                _e.PrintLine(""),
                _e.PrintLine(""),
                _e.PrintLine(""),
                _e.CenterAlign(),
                _e.PrintLine("--------------------------------------------------"),
                _e.LeftAlign(),
                _e.PrintLine("Total: R$"+ order.Total?.ToString("F2")),
                _e.CenterAlign(),
                _e.PrintLine("--------------------------------------------------"),
                _encoding.GetBytes("Obrigado pela preferência, " + order.Name + "!"),
                _e.PrintLine(""),
                _e.PrintLine("Volte sempre!"),
                _e.PrintLine("--------------------------------------------------"),
            ]
        );

        SendToPrinter(print, printer);
        return Task.CompletedTask;
    }

    private void SendToPrinter(byte[] print, NetworkPrinter printer)
    {
        //testPrint(print);

        Setup(printer);
        printer!.Write(print);
        TestPrint(print);
        Teardown(printer);

    }

    private void Setup(NetworkPrinter printer)
    {
        if (printer != null)
        {
            _e.Initialize();
            _e.Enable();

        }
    }
    
    private void Teardown(NetworkPrinter printer)
    {
        if (printer != null)
        {
            _e.FullCut();
            _e.Disable();
        }
    }


    private byte[][] GetPrintItems(List<OrderItemModel> items)
    {
        
        var printItens = new List<byte[]>();
        foreach (var item in items)
        {
            printItens.Add(ByteSplicer.Combine([
                _e.LeftAlign(),
                _encoding.GetBytes("Item: "+ item.Name),
                _e.PrintLine(""),
                _e.PrintLine("Quantidade: "+ item.Quantity),
                _encoding.GetBytes("Preço: R$"+ (item.IsPromotion ?? false ? item.PromotionPrice?.ToString("F2") : item.Price?.ToString("F2"))),
                _e.PrintLine(""),
                _e.PrintLine(""),
                _encoding.GetBytes("Adicionais: "+ (item.Additionals.Count > 0 ? string.Join(", ", item.Additionals.Select(additional => additional.Name)) : "Nenhum adicional selecionado")),
                _e.PrintLine(""),
                _encoding.GetBytes("Observação: "+ (string.IsNullOrEmpty(item.Observation) ? "Nenhuma observação" : item.Observation)),
                _e.PrintLine(""),
                _e.PrintLine(""),
                _e.PrintLine("SubTotal: R$"+ item.Total?.ToString("F2")),
                _e.CenterAlign(),
                _e.PrintLine("--------------------------------------------------"),
            ]));
        }
        return printItens.ToArray();

    }
    
    static void StatusChanged(object sender, EventArgs ps)
    {
        var status = (PrinterStatusEventArgs)ps;
        Console.WriteLine($"Status: {status.IsPrinterOnline}");
        Console.WriteLine($"Has Paper? {status.IsPaperOut}");
        Console.WriteLine($"Paper Running Low? {status.IsPaperLow}");
        Console.WriteLine($"Cash Drawer Open? {status.IsCashDrawerOpen}");
        Console.WriteLine($"Cover Open? {status.IsCoverOpen}");
    }

    private void TestPrint(byte[] print)
    {
        Console.WriteLine("Teste de impressão");
        Console.WriteLine(Encoding.UTF8.GetString(print));
    }
    
    
}