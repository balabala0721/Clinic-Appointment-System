
using ClinicSystem.Models.ViewModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ClinicSystem.Services
{
    public class DrugInventoryPdfService
    {
        public byte[] GeneratePdf(IEnumerable<DrugInventoryReportView> drugs)
        {
            var drugList = drugs.ToList();

            return Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(25);

                    page.Header().Column(column =>
                    {
                        column.Item()
                            .Text("CLINIC MANAGEMENT SYSTEM")
                            .FontSize(18)
                            .Bold();

                        column.Item()
                            .Text("Drug Inventory Report")
                            .FontSize(14)
                            .SemiBold();

                        column.Item()
                            .Text($"Generated Date: {DateTime.Now:dd MMMM yyyy HH:mm}")
                            .FontSize(9)
                            .FontColor(Colors.Grey.Darken1);

                        column.Item().PaddingBottom(10);
                    });

                    page.Content().Column(column =>
                    {
                        column.Item()
                            .Text($"Total Active Drugs: {drugList.Count}")
                            .FontSize(10)
                            .Bold();

                        column.Item().PaddingTop(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(75);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1.5f);
                                columns.ConstantColumn(65);
                                columns.ConstantColumn(55);
                                columns.ConstantColumn(65);
                                columns.ConstantColumn(85);
                            });

                            table.Header(header =>
                            {
                                HeaderCell(header, "Drug ID");
                                HeaderCell(header, "Drug Name");
                                HeaderCell(header, "Category");
                                HeaderCell(header, "Unit Price");
                                HeaderCell(header, "Stock");
                                HeaderCell(header, "Min Stock");
                                HeaderCell(header, "Status");
                            });

                            foreach (var drug in drugList)
                            {
                                DataCell(table, drug.DrugId);
                                DataCell(table, drug.DrugName);
                                DataCell(table, drug.DrugCategory.ToString());
                                DataCell(table, $"RM {drug.UnitPrice:N2}");
                                DataCell(table, drug.StockQuantity.ToString());
                                DataCell(table, drug.MinStockLevel.ToString());

                                var status = drug.StockStatus switch
                                {
                                    ClinicSystem.Models.Entities.StockStatus.OutOfStock => "Out of Stock",
                                    ClinicSystem.Models.Entities.StockStatus.LowStock => "Low Stock",
                                    _ => "In Stock"
                                };

                                DataCell(table, status);
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Page ");
                        text.CurrentPageNumber();
                        text.Span(" of ");
                        text.TotalPages();
                    });
                });
            }).GeneratePdf();
        }

        private static void HeaderCell(
            TableCellDescriptor header, string text)
        {
            header.Cell()
                .Background(Colors.Grey.Darken3)
                .Padding(5)
                .Text(text)
                .FontColor(Colors.White)
                .Bold()
                .FontSize(9);
        }

        private static void DataCell(
            TableDescriptor table, string text)
        {
            table.Cell()
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(5)
                .Text(text)
                .FontSize(8);
        }
    }
}
