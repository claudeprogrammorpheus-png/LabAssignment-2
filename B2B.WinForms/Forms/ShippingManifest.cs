using System.Configuration;
using B2B.Core;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace B2B.WinForms.Forms;

/// <summary>Task 1.3 - "Shipping Manifest" PDF for one order (QuestPDF).</summary>
public static class ShippingManifest
{
    public static Document Build(Order o) => Document.Create(doc => doc.Page(page =>
    {
        page.Size(PageSizes.A4);
        page.Margin(36);
        page.DefaultTextStyle(t => t.FontSize(10)); // bundled Lato font

        page.Header().Column(col =>
        {
            col.Item().Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("B2B Wholesale Distributor").FontSize(18).Bold().FontColor("#1F4E79");
                    c.Item().Text("Central warehouse · 19 Nguyen Huu Tho, District 7, HCMC · (028) 3775 5000").FontSize(9).FontColor(Colors.Grey.Darken1);
                });
                row.ConstantItem(190).AlignRight().Column(c =>
                {
                    c.Item().AlignRight().Text("SHIPPING MANIFEST").FontSize(15).Bold();
                    c.Item().AlignRight().Text($"No. SM-{o.OrderDate:yyyyMM}-{o.OrderID:D5}");
                    c.Item().AlignRight().Text($"Printed: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(9);
                });
            });
            col.Item().PaddingVertical(8).LineHorizontal(1.5f).LineColor("#1F4E79");
        });

        page.Content().Column(col =>
        {
            col.Spacing(12);
            col.Item().Row(row =>
            {
                row.RelativeItem().Border(0.8f).BorderColor(Colors.Grey.Lighten1).Padding(10).Column(c =>
                {
                    c.Item().Text("SHIP TO").FontSize(8).Bold().FontColor(Colors.Grey.Darken1);
                    c.Item().Text(o.Agent?.AgentName ?? "").Bold().FontSize(12);
                    c.Item().Text(o.Agent?.Address ?? "");
                    c.Item().Text($"Tel: {o.Agent?.Phone}");
                });
                row.ConstantItem(12);
                row.RelativeItem().Border(0.8f).BorderColor(Colors.Grey.Lighten1).Padding(10).Column(c =>
                {
                    c.Item().Text("ORDER").FontSize(8).Bold().FontColor(Colors.Grey.Darken1);
                    c.Item().Text($"Order #{o.OrderID}").Bold().FontSize(12);
                    c.Item().Text($"Order date: {o.OrderDate:dd/MM/yyyy HH:mm}");
                    c.Item().Text($"Status: {o.OrderStatus}");
                });
            });

            col.Item().Table(t =>
            {
                t.ColumnsDefinition(c =>
                {
                    c.ConstantColumn(28); c.ConstantColumn(48); c.RelativeColumn(3); c.RelativeColumn(2);
                    c.ConstantColumn(60); c.ConstantColumn(70); c.ConstantColumn(50);
                });
                t.Header(h =>
                {
                    foreach (var head in new[] { "#", "Item ID", "Description", "Pack size", "Qty", "Amount", "Picked" })
                        h.Cell().Background("#1F4E79").Padding(5).Text(head).FontColor(Colors.White).Bold().FontSize(9);
                });
                int n = 1;
                foreach (var d in o.OrderDetails)
                {
                    var bg = n % 2 == 0 ? "#EAF1F8" : "#FFFFFF";
                    t.Cell().Background(bg).Padding(5).Text((n++).ToString());
                    t.Cell().Background(bg).Padding(5).Text(d.ItemID.ToString());
                    t.Cell().Background(bg).Padding(5).Text(d.Item?.ItemName ?? "");
                    t.Cell().Background(bg).Padding(5).Text(d.Item?.Size ?? "");
                    t.Cell().Background(bg).Padding(5).AlignRight().Text(d.Quantity.ToString());
                    t.Cell().Background(bg).Padding(5).AlignRight().Text(d.LineTotal.ToString("N0"));
                    t.Cell().Background(bg).Padding(5).AlignCenter().Text("[   ]");
                }
            });

            col.Item().AlignRight().Text(txt =>
            {
                txt.Span($"Total cartons: {o.OrderDetails.Sum(d => d.Quantity)}    ").Bold();
                txt.Span($"Order value: {o.TotalAmount:N0} VND").Bold();
            });

            col.Item().PaddingTop(30).Row(row =>
            {
                foreach (var who in new[] { "Prepared by (warehouse)", "Driver", "Received by (agent)" })
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().AlignCenter().Text(who).Bold();
                        c.Item().AlignCenter().Text("(signature, full name)").FontSize(8).FontColor(Colors.Grey.Darken1);
                        c.Item().PaddingTop(40).LineHorizontal(0.5f);
                    });
            });
        });

        page.Footer().AlignCenter().Text(t => { t.Span("Page "); t.CurrentPageNumber(); t.Span(" / "); t.TotalPages(); });
    }));

    public static string Export(Order o, string? folder = null)
    {
        folder ??= ConfigurationManager.AppSettings["ManifestFolder"];
        if (string.IsNullOrWhiteSpace(folder))
            folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "B2B Manifests");
        Directory.CreateDirectory(folder);
        var path = Path.Combine(folder, $"ShippingManifest_Order{o.OrderID}.pdf");
        Build(o).GeneratePdf(path);
        return path;
    }
}
