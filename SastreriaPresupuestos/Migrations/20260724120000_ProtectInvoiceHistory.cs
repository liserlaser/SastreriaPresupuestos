using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SastreriaPresupuestos.Data;

#nullable disable

namespace SastreriaPresupuestos.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260724120000_ProtectInvoiceHistory")]
    public partial class ProtectInvoiceHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ArchivedAt",
                table: "Clients",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Clients",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            RebuildInvoicesTable(migrationBuilder, "ON DELETE RESTRICT");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            RebuildInvoicesTable(migrationBuilder, "ON DELETE CASCADE");

            migrationBuilder.DropColumn(name: "ArchivedAt", table: "Clients");
            migrationBuilder.DropColumn(name: "IsArchived", table: "Clients");
        }

        private static void RebuildInvoicesTable(MigrationBuilder migrationBuilder, string clientDeleteBehavior)
        {
            // SQLite cannot alter a foreign key in place. Preserve all invoice data while
            // rebuilding the two related tables with the desired client foreign key.
            migrationBuilder.Sql("PRAGMA foreign_keys = OFF;", suppressTransaction: true);
            migrationBuilder.Sql($"""
                CREATE TABLE "__InvoiceItems_backup" (
                    "Id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    "ProductName" TEXT NOT NULL,
                    "Description" TEXT NOT NULL,
                    "Quantity" INTEGER NOT NULL,
                    "UnitPrice" TEXT NOT NULL,
                    "Total" TEXT NOT NULL,
                    "InvoiceId" INTEGER NOT NULL
                );
                INSERT INTO "__InvoiceItems_backup" ("Id", "ProductName", "Description", "Quantity", "UnitPrice", "Total", "InvoiceId")
                SELECT "Id", "ProductName", "Description", "Quantity", "UnitPrice", "Total", "InvoiceId" FROM "InvoiceItems";
                DROP TABLE "InvoiceItems";

                CREATE TABLE "__Invoices_new" (
                    "Id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    "CreatedAt" TEXT NOT NULL,
                    "Series" TEXT NOT NULL,
                    "Number" INTEGER NOT NULL,
                    "InvoiceNumber" TEXT NOT NULL,
                    "Status" TEXT NOT NULL,
                    "SourceQuoteId" INTEGER NULL,
                    "ClientId" INTEGER NOT NULL,
                    "CorrectsInvoiceId" INTEGER NULL,
                    "Total" TEXT NOT NULL,
                    "Notes" TEXT NOT NULL,
                    CONSTRAINT "FK_Invoices_Clients_ClientId" FOREIGN KEY ("ClientId") REFERENCES "Clients" ("Id") {clientDeleteBehavior},
                    CONSTRAINT "FK_Invoices_Invoices_CorrectsInvoiceId" FOREIGN KEY ("CorrectsInvoiceId") REFERENCES "__Invoices_new" ("Id"),
                    CONSTRAINT "FK_Invoices_Quotes_SourceQuoteId" FOREIGN KEY ("SourceQuoteId") REFERENCES "Quotes" ("Id")
                );
                INSERT INTO "__Invoices_new" ("Id", "CreatedAt", "Series", "Number", "InvoiceNumber", "Status", "SourceQuoteId", "ClientId", "CorrectsInvoiceId", "Total", "Notes")
                SELECT "Id", "CreatedAt", "Series", "Number", "InvoiceNumber", "Status", "SourceQuoteId", "ClientId", "CorrectsInvoiceId", "Total", "Notes" FROM "Invoices";
                DROP TABLE "Invoices";
                ALTER TABLE "__Invoices_new" RENAME TO "Invoices";
                CREATE INDEX "IX_Invoices_ClientId" ON "Invoices" ("ClientId");
                CREATE INDEX "IX_Invoices_CorrectsInvoiceId" ON "Invoices" ("CorrectsInvoiceId");
                CREATE INDEX "IX_Invoices_SourceQuoteId" ON "Invoices" ("SourceQuoteId");

                CREATE TABLE "InvoiceItems" (
                    "Id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    "ProductName" TEXT NOT NULL,
                    "Description" TEXT NOT NULL,
                    "Quantity" INTEGER NOT NULL,
                    "UnitPrice" TEXT NOT NULL,
                    "Total" TEXT NOT NULL,
                    "InvoiceId" INTEGER NOT NULL,
                    CONSTRAINT "FK_InvoiceItems_Invoices_InvoiceId" FOREIGN KEY ("InvoiceId") REFERENCES "Invoices" ("Id") ON DELETE CASCADE
                );
                INSERT INTO "InvoiceItems" ("Id", "ProductName", "Description", "Quantity", "UnitPrice", "Total", "InvoiceId")
                SELECT "Id", "ProductName", "Description", "Quantity", "UnitPrice", "Total", "InvoiceId" FROM "__InvoiceItems_backup";
                DROP TABLE "__InvoiceItems_backup";
                CREATE INDEX "IX_InvoiceItems_InvoiceId" ON "InvoiceItems" ("InvoiceId");
                """, suppressTransaction: true);
            migrationBuilder.Sql("PRAGMA foreign_keys = ON;", suppressTransaction: true);
        }
    }
}
