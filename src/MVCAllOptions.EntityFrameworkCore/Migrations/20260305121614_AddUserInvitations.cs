using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MVCAllOptions.Migrations
{
    /// <inheritdoc />
    public partial class AddUserInvitations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmbedderApiBaseUrl",
                table: "AIManagementWorkspaces",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmbedderApiKey",
                table: "AIManagementWorkspaces",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmbedderModelName",
                table: "AIManagementWorkspaces",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmbedderProvider",
                table: "AIManagementWorkspaces",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VectorStoreProvider",
                table: "AIManagementWorkspaces",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VectorStoreSettings",
                table: "AIManagementWorkspaces",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupportedEmbedderProviders",
                table: "AIManagementApplicationAIProviders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SupportedVectorStoreProviders",
                table: "AIManagementApplicationAIProviders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "Leaved",
                table: "AbpUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "PropertyTypeFullName",
                table: "AbpEntityPropertyChanges",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<string>(
                name: "EntityTypeFullName",
                table: "AbpEntityChanges",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.CreateTable(
                name: "AbpUserInvitations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InviterTenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    InviteeEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InvitationDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    AssignedRoles = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpUserInvitations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AIManagementDocumentChunks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkspaceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DataSourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChunkIndex = table.Column<int>(type: "int", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIManagementDocumentChunks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AIManagementMcpServers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    TransportType = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Command = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    Arguments = table.Column<string>(type: "nvarchar(max)", maxLength: 4096, nullable: true),
                    WorkingDirectory = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    EnvironmentVariables = table.Column<string>(type: "nvarchar(max)", maxLength: 8192, nullable: true),
                    Endpoint = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    Headers = table.Column<string>(type: "nvarchar(max)", maxLength: 8192, nullable: true),
                    AuthType = table.Column<int>(type: "int", nullable: false),
                    ApiKey = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    CustomAuthHeaderName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    CustomAuthHeaderValue = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIManagementMcpServers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AIManagementWorkspaceDataSources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkspaceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    BlobName = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    IsProcessed = table.Column<bool>(type: "bit", nullable: false),
                    ProcessedTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastError = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    LastErrorTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIManagementWorkspaceDataSources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AIManagementWorkspaceMcpServers",
                columns: table => new
                {
                    WorkspaceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    McpServerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIManagementWorkspaceMcpServers", x => new { x.WorkspaceId, x.McpServerId });
                    table.ForeignKey(
                        name: "FK_AIManagementWorkspaceMcpServers_AIManagementWorkspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalTable: "AIManagementWorkspaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AIManagementDocumentChunks_DataSourceId",
                table: "AIManagementDocumentChunks",
                column: "DataSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_AIManagementDocumentChunks_WorkspaceId",
                table: "AIManagementDocumentChunks",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_AIManagementMcpServers_Name",
                table: "AIManagementMcpServers",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AIManagementWorkspaceDataSources_IsProcessed",
                table: "AIManagementWorkspaceDataSources",
                column: "IsProcessed");

            migrationBuilder.CreateIndex(
                name: "IX_AIManagementWorkspaceDataSources_WorkspaceId",
                table: "AIManagementWorkspaceDataSources",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_AIManagementWorkspaceMcpServers_McpServerId",
                table: "AIManagementWorkspaceMcpServers",
                column: "McpServerId");

            migrationBuilder.CreateIndex(
                name: "IX_AIManagementWorkspaceMcpServers_WorkspaceId",
                table: "AIManagementWorkspaceMcpServers",
                column: "WorkspaceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AbpUserInvitations");

            migrationBuilder.DropTable(
                name: "AIManagementDocumentChunks");

            migrationBuilder.DropTable(
                name: "AIManagementMcpServers");

            migrationBuilder.DropTable(
                name: "AIManagementWorkspaceDataSources");

            migrationBuilder.DropTable(
                name: "AIManagementWorkspaceMcpServers");

            migrationBuilder.DropColumn(
                name: "EmbedderApiBaseUrl",
                table: "AIManagementWorkspaces");

            migrationBuilder.DropColumn(
                name: "EmbedderApiKey",
                table: "AIManagementWorkspaces");

            migrationBuilder.DropColumn(
                name: "EmbedderModelName",
                table: "AIManagementWorkspaces");

            migrationBuilder.DropColumn(
                name: "EmbedderProvider",
                table: "AIManagementWorkspaces");

            migrationBuilder.DropColumn(
                name: "VectorStoreProvider",
                table: "AIManagementWorkspaces");

            migrationBuilder.DropColumn(
                name: "VectorStoreSettings",
                table: "AIManagementWorkspaces");

            migrationBuilder.DropColumn(
                name: "SupportedEmbedderProviders",
                table: "AIManagementApplicationAIProviders");

            migrationBuilder.DropColumn(
                name: "SupportedVectorStoreProviders",
                table: "AIManagementApplicationAIProviders");

            migrationBuilder.DropColumn(
                name: "Leaved",
                table: "AbpUsers");

            migrationBuilder.AlterColumn<string>(
                name: "PropertyTypeFullName",
                table: "AbpEntityPropertyChanges",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(512)",
                oldMaxLength: 512);

            migrationBuilder.AlterColumn<string>(
                name: "EntityTypeFullName",
                table: "AbpEntityChanges",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(512)",
                oldMaxLength: 512);
        }
    }
}
