using System.Net;
using System.Text;
using ESCPOS_NET;
using ESCPOS_NET.Emitters;
using ESCPOS_NET.Utilities;
using self_service_core.Models;

namespace self_service_core.Services;

public class PrinterService : IPrinterService
{
    
    private readonly BasePrinter? _printer;
    private readonly ICommandEmitter e;
    
    public PrinterService(PrinterModel printer)
    {
        // verify if host is IP
        if (!IPAddress.TryParse(printer.Host, out _))
        {
            _printer = new SerialPrinter(portName: printer.Host, baudRate: int.Parse(printer.Port));
        }
        else
        {
            _printer = new NetworkPrinter(new NetworkPrinterSettings() { ConnectionString = $"{printer.Host}:{printer.Port}" });
        }

        e = new EPSON();
        
        
        var factory = LoggerFactory.Create(b => b.AddConsole().SetMinimumLevel(LogLevel.Information));
        var logger = factory.CreateLogger<Program>();
        ESCPOS_NET.Logging.Logger = logger;
    }


    public Task Print(OrderItemModel orderItem)
    {
        //PT-BR
        var encoding = System.Text.Encoding.GetEncoding("UTF-8");
        var print = ByteSplicer.Combine([
                e.CenterAlign(),
                e.PrintLine("--------------------------------------------------"),
                e.PrintLine("Pedido "+ orderItem.ItemId),
                e.PrintLine("Data e Hora: "+ orderItem.CreatedAt.ToString("dd/MM/yyyy HH:mm")),
                e.PrintLine("--------------------------------------------------"),
                e.PrintLine("Mesa: "+ orderItem.CardNumber),
                e.PrintLine("--------------------------------------------------"),
                e.LeftAlign(),
                encoding.GetBytes("Item: "+ orderItem.Name),
                e.PrintLine(""),
                e.PrintLine("Quantidade: "+ orderItem.Quantity),
                encoding.GetBytes("Preço: R$ "+ orderItem.Price?.ToString("F2")),
                e.PrintLine(""),
                e.PrintLine(""),
                encoding.GetBytes("Adicionais: "+ (orderItem.Additionals.Count > 0 ? string.Join(", ", orderItem.Additionals.Select(additional => additional.Name)) : "Nenhum adicional selecionado")), 
                e.PrintLine(""),
                encoding.GetBytes("Observação: "+ (string.IsNullOrEmpty(orderItem.Observation) ? "Nenhuma observação" : orderItem.Observation)),
                e.PrintLine(""),
                e.CenterAlign(),
                e.PrintLine("--------------------------------------------------"),
                e.LeftAlign(),
                e.PrintLine("Total: R$ "+ orderItem.Total?.ToString("F2")),
                e.CenterAlign(),
                e.PrintLine("--------------------------------------------------"),
            ]
        );
        
        SendToPrinter(print);
        return Task.CompletedTask;
    }
    
    public Task Print(OrderModel order)
    {
        var encoding = System.Text.Encoding.GetEncoding("UTF-8");
        var print = ByteSplicer.Combine([
                e.CenterAlign(),
                e.PrintLine("--------------------------------------------------"),
                e.PrintLine("Comanda "+ order.OrderId),
                e.PrintLine("Data e Hora: "+ order.CreatedAt.ToString("dd/MM/yyyy HH:mm")),
                e.PrintLine("--------------------------------------------------"),
                e.PrintLine("Mesa: "+ order.CardNumber),
                e.PrintLine("--------------------------------------------------"),
                e.LeftAlign(),
                ..GetPrintItems(order.Items),
                e.PrintLine(""),
                e.PrintLine(""),
                e.PrintLine(""),
                e.CenterAlign(),
                e.PrintLine("--------------------------------------------------"),
                e.LeftAlign(),
                e.PrintLine("Total: R$ "+ order.Total?.ToString("F2")),
                e.CenterAlign(),
                e.PrintLine("--------------------------------------------------"),
                encoding.GetBytes("Obrigado pela preferência!"),
                e.PrintLine(""),
                e.PrintLine("Volte sempre!"),
                e.PrintLine("--------------------------------------------------"),
            ]
        );
        
        SendToPrinter(print);
        return Task.CompletedTask;
    }

    private void SendToPrinter(byte[] print)
    {
        //testPrint(print);

        if (_printer is NetworkPrinter)
        {
            _printer.Write(e.Initialize());
            _printer.Write(e.Enable());
            _printer.Write(e.EnableAutomaticStatusBack());
            Setup(true);
            
            var isOnline = true;
                           //_printer.GetStatus().IsPrinterOnline ?? false;

            if (!isOnline)
            {
                throw new Exception("Impressora offline");
            }
            _printer.Write(print);
        }
        else
        {
            var status = _printer?.GetStatus();
            if (status == null || (!status.IsPrinterOnline ?? true))
            {
                throw new Exception("Impressora offline");
            }
            _printer!.Write(print);
        }


    }

    private void Setup(bool enableStatusBackMonitoring)
        {
            if (_printer != null)
            {
                // Only register status monitoring once.
                //if (!_hasEnabledStatusMonitoring)
                //{
                //    _printer.StatusChanged += StatusChanged;
                //    _hasEnabledStatusMonitoring = true;
                //}
                _printer?.Write(e.Initialize());
                _printer?.Write(e.Enable());
                if (enableStatusBackMonitoring)
                {
                    _printer?.Write(e.EnableAutomaticStatusBack());
                }
            }
        }


    private byte[][] GetPrintItems(List<OrderItemModel> items)
    {
        var encoding = System.Text.Encoding.GetEncoding("UTF-8");
        var printItens = new List<byte[]>();
        foreach (var item in items)
        {
            printItens.Add(ByteSplicer.Combine([
                e.LeftAlign(),
                encoding.GetBytes("Item: "+ item.Name),
                e.PrintLine(""),
                e.PrintLine("Quantidade: "+ item.Quantity),
                encoding.GetBytes("Preço: R$ "+ item.Price?.ToString("F2")),
                e.PrintLine(""),
                e.PrintLine(""),
                encoding.GetBytes("Adicionais: "+ (item.Additionals.Count > 0 ? string.Join(", ", item.Additionals.Select(additional => additional.Name)) : "Nenhum adicional selecionado")), 
                e.PrintLine(""),
                encoding.GetBytes("Observação: "+ (string.IsNullOrEmpty(item.Observation) ? "Nenhuma observação" : item.Observation)),
                e.PrintLine(""),
                e.PrintLine(""),
                e.PrintLine("Total: R$"+ item.Total?.ToString("F2")),
                e.CenterAlign(),
                e.PrintLine("--------------------------------------------------"),
            ]));
        }
        return printItens.ToArray();
        
    }

    private void testPrint(byte[] print)
    {
        Console.WriteLine("Teste de impressão");
        Console.WriteLine(Encoding.UTF8.GetString(print));
    }
    
}