using System.Net;
using System.Text;
using ESCPOS_NET;
using ESCPOS_NET.Emitters;
using ESCPOS_NET.Utilities;
using self_service_core.Models;

namespace self_service_core.Services;

public class PrinterService : IPrinterService
{
    
    private readonly ImmediateNetworkPrinter? _networkPrinter;
    private readonly SerialPrinter? _serialPrinter;
    
    public PrinterService(PrinterModel printer)
    {
        // verify if host is IP
        if (!IPAddress.TryParse(printer.Host, out _))
        {
            _serialPrinter = new SerialPrinter(portName: printer.Host, baudRate: int.Parse(printer.Port));
        }
        else
        {
            _networkPrinter = new ImmediateNetworkPrinter(new ImmediateNetworkPrinterSettings() { ConnectionString = $"{printer.Host}:{printer.Port}", PrinterName = "TestPrinter" });
        }
    }


    public async Task Print(OrderItemModel orderItem)
    {
        //PT-BR
        var encoding = System.Text.Encoding.GetEncoding("UTF-8");
        var e = new EPSON();
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
        
        await SendToPrinter(print, e);
       
    }
    
    public async Task Print(OrderModel order)
    {
        var encoding = System.Text.Encoding.GetEncoding("UTF-8");
        var e = new EPSON();
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
                e.PrintLine("Total: R$ "+ order.Total),
                e.CenterAlign(),
                e.PrintLine("--------------------------------------------------"),
                encoding.GetBytes("Obrigado pela preferência!"),
                e.PrintLine(""),
                e.PrintLine("Volte sempre!"),
                e.PrintLine("--------------------------------------------------"),
            ]
        );
        
        await SendToPrinter(print, e);
    }
    
    private async Task SendToPrinter(byte[] print, ICommandEmitter e)
    {
        testPrint(print);
        
        if (_networkPrinter != null)
        {
            var isOnline = await _networkPrinter.GetOnlineStatus(e);
            if (!isOnline)
            {
                //throw new Exception("Impressora offline");
            }
            
            await _networkPrinter.WriteAsync(print);
        }
        else
        {
            var status =  _serialPrinter?.GetStatus();
            if (status == null || (!status.IsPrinterOnline ?? true))
            {
                throw new Exception("Impressora offline");
            }
            _serialPrinter!.Write(print);
        }
        
        
    }
    

    private byte[][] GetPrintItems(List<OrderItemModel> items)
    {
        var encoding = System.Text.Encoding.GetEncoding("UTF-8");
        var e = new EPSON();
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