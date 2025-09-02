using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlanningInvestment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInvestmentEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Investments",
                table: "Investments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Debts",
                table: "Debts");

            migrationBuilder.RenameTable(
                name: "Investments",
                newName: "investments");

            migrationBuilder.RenameTable(
                name: "Debts",
                newName: "debts");

            migrationBuilder.RenameColumn(
                name: "Symbol",
                table: "investments",
                newName: "symbol");

            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "investments",
                newName: "quantity");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "investments",
                newName: "notes");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "investments",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "investments",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "investments",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "UpdateBy",
                table: "investments",
                newName: "update_by");

            migrationBuilder.RenameColumn(
                name: "PurchasePrice",
                table: "investments",
                newName: "purchase_price");

            migrationBuilder.RenameColumn(
                name: "PurchaseDate",
                table: "investments",
                newName: "purchase_date");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "investments",
                newName: "is_deleted");

            migrationBuilder.RenameColumn(
                name: "InvestmentType",
                table: "investments",
                newName: "investment_type");

            migrationBuilder.RenameColumn(
                name: "CurrentMarketPrice",
                table: "investments",
                newName: "current_market_price");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "investments",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "CreateBy",
                table: "investments",
                newName: "create_by");

            migrationBuilder.RenameIndex(
                name: "IX_Investments_Id",
                table: "investments",
                newName: "ix_investments_id");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "debts",
                newName: "notes");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "debts",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "debts",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Creditor",
                table: "debts",
                newName: "creditor");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "debts",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "debts",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "debts",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "UpdateBy",
                table: "debts",
                newName: "update_by");

            migrationBuilder.RenameColumn(
                name: "TargetPayoffDate",
                table: "debts",
                newName: "target_payoff_date");

            migrationBuilder.RenameColumn(
                name: "OriginalAmount",
                table: "debts",
                newName: "original_amount");

            migrationBuilder.RenameColumn(
                name: "MinimumPayment",
                table: "debts",
                newName: "minimum_payment");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "debts",
                newName: "is_deleted");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "debts",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "InterestRate",
                table: "debts",
                newName: "interest_rate");

            migrationBuilder.RenameColumn(
                name: "DueDate",
                table: "debts",
                newName: "due_date");

            migrationBuilder.RenameColumn(
                name: "DebtType",
                table: "debts",
                newName: "debt_type");

            migrationBuilder.RenameColumn(
                name: "CurrentBalance",
                table: "debts",
                newName: "current_balance");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "debts",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "CreateBy",
                table: "debts",
                newName: "create_by");

            migrationBuilder.RenameColumn(
                name: "AccountNumber",
                table: "debts",
                newName: "account_number");

            migrationBuilder.RenameIndex(
                name: "IX_Debts_Id",
                table: "debts",
                newName: "ix_debts_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_investments",
                table: "investments",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_debts",
                table: "debts",
                column: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_investments",
                table: "investments");

            migrationBuilder.DropPrimaryKey(
                name: "pk_debts",
                table: "debts");

            migrationBuilder.RenameTable(
                name: "investments",
                newName: "Investments");

            migrationBuilder.RenameTable(
                name: "debts",
                newName: "Debts");

            migrationBuilder.RenameColumn(
                name: "symbol",
                table: "Investments",
                newName: "Symbol");

            migrationBuilder.RenameColumn(
                name: "quantity",
                table: "Investments",
                newName: "Quantity");

            migrationBuilder.RenameColumn(
                name: "notes",
                table: "Investments",
                newName: "Notes");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Investments",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "Investments",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "Investments",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "update_by",
                table: "Investments",
                newName: "UpdateBy");

            migrationBuilder.RenameColumn(
                name: "purchase_price",
                table: "Investments",
                newName: "PurchasePrice");

            migrationBuilder.RenameColumn(
                name: "purchase_date",
                table: "Investments",
                newName: "PurchaseDate");

            migrationBuilder.RenameColumn(
                name: "is_deleted",
                table: "Investments",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "investment_type",
                table: "Investments",
                newName: "InvestmentType");

            migrationBuilder.RenameColumn(
                name: "current_market_price",
                table: "Investments",
                newName: "CurrentMarketPrice");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Investments",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "create_by",
                table: "Investments",
                newName: "CreateBy");

            migrationBuilder.RenameIndex(
                name: "ix_investments_id",
                table: "Investments",
                newName: "IX_Investments_Id");

            migrationBuilder.RenameColumn(
                name: "notes",
                table: "Debts",
                newName: "Notes");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Debts",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "Debts",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "creditor",
                table: "Debts",
                newName: "Creditor");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Debts",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "Debts",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "Debts",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "update_by",
                table: "Debts",
                newName: "UpdateBy");

            migrationBuilder.RenameColumn(
                name: "target_payoff_date",
                table: "Debts",
                newName: "TargetPayoffDate");

            migrationBuilder.RenameColumn(
                name: "original_amount",
                table: "Debts",
                newName: "OriginalAmount");

            migrationBuilder.RenameColumn(
                name: "minimum_payment",
                table: "Debts",
                newName: "MinimumPayment");

            migrationBuilder.RenameColumn(
                name: "is_deleted",
                table: "Debts",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "Debts",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "interest_rate",
                table: "Debts",
                newName: "InterestRate");

            migrationBuilder.RenameColumn(
                name: "due_date",
                table: "Debts",
                newName: "DueDate");

            migrationBuilder.RenameColumn(
                name: "debt_type",
                table: "Debts",
                newName: "DebtType");

            migrationBuilder.RenameColumn(
                name: "current_balance",
                table: "Debts",
                newName: "CurrentBalance");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Debts",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "create_by",
                table: "Debts",
                newName: "CreateBy");

            migrationBuilder.RenameColumn(
                name: "account_number",
                table: "Debts",
                newName: "AccountNumber");

            migrationBuilder.RenameIndex(
                name: "ix_debts_id",
                table: "Debts",
                newName: "IX_Debts_Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Investments",
                table: "Investments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Debts",
                table: "Debts",
                column: "Id");
        }
    }
}
