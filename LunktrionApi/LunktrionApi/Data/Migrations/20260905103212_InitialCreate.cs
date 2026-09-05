using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LunktrionApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "devices",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    device_uuid = table.Column<string>(type: "text", nullable: false),
                    device_name = table.Column<string>(type: "text", nullable: false),
                    operating_system_type = table.Column<string>(type: "text", nullable: false),
                    operating_system_name = table.Column<string>(type: "text", nullable: false),
                    device_manufacturer = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_devices", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "device_cpu_specifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    device_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    number_of_cores = table.Column<short>(type: "smallint", nullable: false),
                    number_of_logical_processors = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_device_cpu_specifications", x => x.id);
                    table.ForeignKey(
                        name: "fk_device_cpu_specifications_devices_device_id",
                        column: x => x.device_id,
                        principalTable: "devices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "device_drive_specifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    device_id = table.Column<Guid>(type: "uuid", nullable: false),
                    caption = table.Column<string>(type: "text", nullable: false),
                    total_size = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_device_drive_specifications", x => x.id);
                    table.ForeignKey(
                        name: "fk_device_drive_specifications_devices_device_id",
                        column: x => x.device_id,
                        principalTable: "devices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "device_gpu_specifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    device_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    video_ram = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_device_gpu_specifications", x => x.id);
                    table.ForeignKey(
                        name: "fk_device_gpu_specifications_devices_device_id",
                        column: x => x.device_id,
                        principalTable: "devices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "device_ram_specifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    device_id = table.Column<Guid>(type: "uuid", nullable: false),
                    manufacturer = table.Column<string>(type: "text", nullable: false),
                    size = table.Column<long>(type: "bigint", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    speed = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_device_ram_specifications", x => x.id);
                    table.ForeignKey(
                        name: "fk_device_ram_specifications_devices_device_id",
                        column: x => x.device_id,
                        principalTable: "devices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_device_cpu_specifications_device_id",
                table: "device_cpu_specifications",
                column: "device_id");

            migrationBuilder.CreateIndex(
                name: "is_device_drive_specifications_device_id",
                table: "device_drive_specifications",
                column: "device_id");

            migrationBuilder.CreateIndex(
                name: "ix_device_gpu_specifications_device_id",
                table: "device_gpu_specifications",
                column: "device_id");

            migrationBuilder.CreateIndex(
                name: "is_device_ram_specifications_device_id",
                table: "device_ram_specifications",
                column: "device_id");

            migrationBuilder.CreateIndex(
                name: "ix_devices_operating_system_type",
                table: "devices",
                column: "operating_system_type");

            migrationBuilder.CreateIndex(
                name: "ux_devices_device_uuid",
                table: "devices",
                column: "device_uuid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "device_cpu_specifications");

            migrationBuilder.DropTable(
                name: "device_drive_specifications");

            migrationBuilder.DropTable(
                name: "device_gpu_specifications");

            migrationBuilder.DropTable(
                name: "device_ram_specifications");

            migrationBuilder.DropTable(
                name: "devices");
        }
    }
}
