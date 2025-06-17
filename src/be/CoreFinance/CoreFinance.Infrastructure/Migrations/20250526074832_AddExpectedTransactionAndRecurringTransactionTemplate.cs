using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreFinance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExpectedTransactionAndRecurringTransactionTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "account_type",
                table: "accounts",
                newName: "type");

            migrationBuilder.RenameColumn(
                name: "account_name",
                table: "accounts",
                newName: "name");

            migrationBuilder.CreateTable(
                name: "recurring_transaction_templates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    transaction_type = table.Column<int>(type: "integer", nullable: false),
                    category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    frequency = table.Column<int>(type: "integer", nullable: false),
                    custom_interval_days = table.Column<int>(type: "integer", nullable: true),
                    next_execution_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    cron_expression = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    auto_generate = table.Column<bool>(type: "boolean", nullable: false),
                    days_in_advance = table.Column<int>(type: "integer", nullable: false),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    create_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    update_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    create_by = table.Column<string>(type: "text", nullable: true),
                    update_by = table.Column<string>(type: "text", nullable: true),
                    deleted = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_recurring_transaction_templates", x => x.id);
                    table.ForeignKey(
                        name: "fk_recurring_transaction_templates_accounts_account_id",
                        column: x => x.account_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "expected_transactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    recurring_transaction_template_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    actual_transaction_id = table.Column<Guid>(type: "uuid", nullable: true),
                    expected_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expected_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    transaction_type = table.Column<int>(type: "integer", nullable: false),
                    category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    is_adjusted = table.Column<bool>(type: "boolean", nullable: false),
                    original_amount = table.Column<decimal>(type: "numeric", nullable: true),
                    adjustment_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    generated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    create_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    update_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    create_by = table.Column<string>(type: "text", nullable: true),
                    update_by = table.Column<string>(type: "text", nullable: true),
                    deleted = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_expected_transactions", x => x.id);
                    table.ForeignKey(
                        name: "fk_expected_transactions_accounts_account_id",
                        column: x => x.account_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_expected_transactions_recurring_transaction_templates_recur",
                        column: x => x.recurring_transaction_template_id,
                        principalTable: "recurring_transaction_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_expected_transactions_transactions_actual_transaction_id",
                        column: x => x.actual_transaction_id,
                        principalTable: "transactions",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_expected_transactions_account_id",
                table: "expected_transactions",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "ix_expected_transactions_actual_transaction_id",
                table: "expected_transactions",
                column: "actual_transaction_id");

            migrationBuilder.CreateIndex(
                name: "ix_expected_transactions_id",
                table: "expected_transactions",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_expected_transactions_recurring_transaction_template_id",
                table: "expected_transactions",
                column: "recurring_transaction_template_id");

            migrationBuilder.CreateIndex(
                name: "ix_recurring_transaction_templates_account_id",
                table: "recurring_transaction_templates",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "ix_recurring_transaction_templates_id",
                table: "recurring_transaction_templates",
                column: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "expected_transactions");

            migrationBuilder.DropTable(
                name: "recurring_transaction_templates");

            migrationBuilder.RenameColumn(
                name: "type",
                table: "accounts",
                newName: "account_type");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "accounts",
                newName: "account_name");
        }
    }
}
