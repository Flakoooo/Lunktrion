using LunktrionApi.Models.Entities;
using LunktrionApi.Utils;
using Microsoft.EntityFrameworkCore;

namespace LunktrionApi.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        private static string DefaultUuidSql => "gen_random_uuid()";

        public DbSet<Device> Devices { get; set; }
        public DbSet<DeviceCpuSpecification> DeviceCpuSpecifications { get; set; }
        public DbSet<DeviceGpuSpecification> DeviceGpuSpecifications { get; set; }
        public DbSet<DeviceRamSpecification> DeviceRamSpecifications { get; set; }
        public DbSet<DeviceDriveSpecification> DeviceDriveSpecifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Device>(entity =>
            {
                entity.ToTable(Device.TableName);

                // uuid DEFAULT gen_random_uuid()
                entity.Property(d => d.Id)
                      .HasColumnType("uuid")
                      .HasColumnName(Converters.ConvertNameToSnakeCase(nameof(Device.Id)))
                      .HasDefaultValueSql(DefaultUuidSql);
                                
                entity.Property(d => d.DeviceUUID)
                      .HasColumnType("text")
                      .HasColumnName(Device.DeviceUUIDColumnName)
                      .IsRequired(); // NOT NULL

                entity.Property(d => d.DeviceName)
                      .HasColumnType("text")
                      .HasColumnName(Converters.ConvertNameToSnakeCase(nameof(Device.DeviceName)))
                      .IsRequired(); // NOT NULL

                entity.Property(d => d.OperatingSystemType)
                      .HasColumnType("text")
                      .HasColumnName(Device.OperatingSystemTypeColumnName)
                      .HasConversion(
                          value => value.ToString(),
                          value => Converters.ParseOperatingSystem(value)
                      )
                      .IsRequired(); // NOT NULL

                entity.Property(d => d.OperatingSystemName)
                      .HasColumnType("text")  
                      .HasColumnName(Converters.ConvertNameToSnakeCase(nameof(Device.OperatingSystemName)))
                      .IsRequired(); // NOT NULL

                entity.Property(d => d.DeviceManufacturer)
                      .HasColumnType("text")
                      .HasColumnName(Converters.ConvertNameToSnakeCase(nameof(Device.DeviceManufacturer)))
                      .IsRequired(); // NOT NULL

                // CONSTRAINT "pk_devices" PRIMARY KEY ("id")
                entity.HasKey(d => d.Id)
                      .HasName($"pk_{Device.TableName}");

                // CREATE UNIQUE INDEX "ix_devices_device_uuid" ON "devices" ("device_uuid");
                entity.HasIndex(d => d.DeviceUUID)
                      .HasDatabaseName($"ux_{Device.TableName}_{Device.DeviceUUIDColumnName}")
                      .IsUnique();

                // CREATE INDEX "ix_operating_system_type" on "devices" ("operating_system_type");
                entity.HasIndex(d => d.OperatingSystemType)
                      .HasDatabaseName($"ix_{Device.TableName}_{Device.OperatingSystemTypeColumnName}");
            });

            modelBuilder.Entity<DeviceCpuSpecification>(entity =>
            {
                entity.ToTable(DeviceCpuSpecification.TableName);

                // uuid DEFAULT gen_random_uuid()
                entity.Property(dcs => dcs.Id)
                      .HasColumnType("uuid")
                      .HasColumnName(Converters.ConvertNameToSnakeCase(nameof(DeviceCpuSpecification.Id)))
                      .HasDefaultValueSql(DefaultUuidSql);

                entity.Property(dcs => dcs.DeviceId)
                      .HasColumnType("uuid")
                      .HasColumnName(DeviceCpuSpecification.DeviceIdColumnName)
                      .IsRequired();

                entity.Property(dcs => dcs.Name)
                      .HasColumnType("text")
                      .HasColumnName(Converters.ConvertNameToSnakeCase(nameof(DeviceCpuSpecification.Name)))
                      .IsRequired();

                entity.Property(dcs => dcs.NumberOfCores)
                      .HasColumnType("smallint")
                      .HasColumnName(Converters.ConvertNameToSnakeCase(nameof(DeviceCpuSpecification.NumberOfCores)))
                      .IsRequired();

                entity.Property(dcs => dcs.NumberOfLogicalProcessors)
                      .HasColumnType("smallint")
                      .HasColumnName(Converters.ConvertNameToSnakeCase(nameof(DeviceCpuSpecification.NumberOfLogicalProcessors)))
                      .IsRequired();

                // CONSTRAINT $"pk_{tableName}" PRIMARY KEY ("id")
                entity.HasKey(dcs => dcs.Id)
                      .HasName($"pk_{DeviceCpuSpecification.TableName}");

                // CREATE INDEX $"ix_{tableName}_{columnDeviceIdName}" on $"{tableName}" ($"{columnDeviceIdName}");
                entity.HasIndex(dcs => dcs.DeviceId)
                      .HasDatabaseName($"ix_{DeviceCpuSpecification.TableName}_{DeviceCpuSpecification.DeviceIdColumnName}");

                // CONSTRAINT $"fk_{tableName}_devices_{columnDeviceIdName}" FOREIGN KEY ("device_id") REFERENCES "devices" ("id") ON DELETE CASCADE
                entity.HasOne(dcs => dcs.Device)
                      .WithMany(d => d.CpuSpecifications)
                      .HasForeignKey(s => s.DeviceId)
                      .OnDelete(DeleteBehavior.Cascade)
                      .HasConstraintName($"fk_{DeviceCpuSpecification.TableName}_{Device.TableName}_{DeviceCpuSpecification.DeviceIdColumnName}");
            });

            modelBuilder.Entity<DeviceGpuSpecification>(entity =>
            {
                entity.ToTable(DeviceGpuSpecification.TableName);


                entity.Property(dgs => dgs.Id)
                      .HasColumnType("uuid")
                      .HasColumnName(Converters.ConvertNameToSnakeCase(nameof(DeviceGpuSpecification.Id)))
                      .HasDefaultValueSql(DefaultUuidSql);

                entity.Property(dgs => dgs.DeviceId)
                      .HasColumnType("uuid")
                      .HasColumnName(DeviceGpuSpecification.DeviceIdColumnName)
                      .IsRequired();

                entity.Property(dgs => dgs.Name)
                      .HasColumnType("text")
                      .HasColumnName(Converters.ConvertNameToSnakeCase(nameof(DeviceGpuSpecification.Name)))
                      .IsRequired();

                entity.Property(dgs => dgs.VideoRam)
                      .HasColumnType("bigint")
                      .HasColumnName(Converters.ConvertNameToSnakeCase(nameof(DeviceGpuSpecification.VideoRam)))
                      .HasConversion(value => (long)value, value => (ulong)value)
                      .IsRequired();

                entity.HasKey(dgs => dgs.Id) 
                      .HasName($"pk_{DeviceGpuSpecification.TableName}");

                entity.HasIndex(dgs => dgs.DeviceId)
                      .HasDatabaseName($"ix_{DeviceGpuSpecification.TableName}_{DeviceGpuSpecification.DeviceIdColumnName}");

                entity.HasOne(dgs => dgs.Device)
                      .WithMany(d => d.GpuSpecifications)
                      .HasForeignKey(s => s.DeviceId)
                      .OnDelete(DeleteBehavior.Cascade)
                      .HasConstraintName($"fk_{DeviceGpuSpecification.TableName}_{Device.TableName}_{DeviceGpuSpecification.DeviceIdColumnName}");
            });

            modelBuilder.Entity<DeviceRamSpecification>(entity =>
            {
                entity.ToTable(DeviceRamSpecification.TableName);

                entity.Property(drs => drs.Id)
                      .HasColumnType("uuid")
                      .HasColumnName(Converters.ConvertNameToSnakeCase(nameof(DeviceRamSpecification.Id)))
                      .HasDefaultValueSql(DefaultUuidSql);

                entity.Property(drs => drs.DeviceId)
                      .HasColumnType("uuid")
                      .HasColumnName(DeviceRamSpecification.DeviceIdColumnName)
                      .IsRequired();

                entity.Property(drs => drs.Manufacturer)
                      .HasColumnType("text")
                      .HasColumnName(Converters.ConvertNameToSnakeCase(nameof(DeviceRamSpecification.Manufacturer)))
                      .IsRequired();

                entity.Property(drs => drs.Size)
                      .HasColumnType("bigint")
                      .HasColumnName(Converters.ConvertNameToSnakeCase(nameof(DeviceRamSpecification.Size)))
                      .HasConversion(value => (long)value, value => (ulong)value)
                      .IsRequired();

                entity.Property(drs => drs.Type)
                      .HasColumnType("text")
                      .HasColumnName(Converters.ConvertNameToSnakeCase(nameof(DeviceRamSpecification.Type)))
                      .IsRequired();

                entity.Property(drs => drs.Speed)
                      .HasColumnType("integer")
                      .HasColumnName(Converters.ConvertNameToSnakeCase(nameof(DeviceRamSpecification.Speed)))
                      .IsRequired();

                entity.HasKey(drs => drs.Id)
                      .HasName($"pk_{DeviceRamSpecification.TableName}");

                entity.HasIndex(drs => drs.DeviceId)
                      .HasDatabaseName($"is_{DeviceRamSpecification.TableName}_{DeviceRamSpecification.DeviceIdColumnName}");

                entity.HasOne(drs => drs.Device)
                      .WithMany(d => d.RamSpecifications)
                      .HasForeignKey(s => s.DeviceId)
                      .OnDelete(DeleteBehavior.Cascade)
                      .HasConstraintName($"fk_{DeviceRamSpecification.TableName}_{Device.TableName}_{DeviceRamSpecification.DeviceIdColumnName}");
            });

            modelBuilder.Entity<DeviceDriveSpecification>(entity =>
            {
                entity.ToTable(DeviceDriveSpecification.TableName);

                entity.Property(dds => dds.Id)
                      .HasColumnType("uuid")
                      .HasColumnName(Converters.ConvertNameToSnakeCase(nameof(DeviceDriveSpecification.Id)))
                      .HasDefaultValueSql(DefaultUuidSql);

                entity.Property(dds => dds.DeviceId)
                      .HasColumnType("uuid")
                      .HasColumnName(Converters.ConvertNameToSnakeCase(DeviceDriveSpecification.DeviceIdColumnName))
                      .IsRequired();

                entity.Property(dds => dds.Caption)
                      .HasColumnType("text")
                      .HasColumnName(Converters.ConvertNameToSnakeCase(nameof(DeviceDriveSpecification.Caption)))
                      .IsRequired();

                entity.Property(dds => dds.TotalSize)
                      .HasColumnType("bigint")
                      .HasColumnName(Converters.ConvertNameToSnakeCase(nameof(DeviceDriveSpecification.TotalSize)))
                      .HasConversion(value => (long)value, value => (ulong)value)
                      .IsRequired();

                entity.HasKey(dds => dds.Id)
                      .HasName($"pk_{DeviceDriveSpecification.TableName}");

                entity.HasIndex(dds => dds.DeviceId)
                      .HasDatabaseName($"is_{DeviceDriveSpecification.TableName}_{DeviceDriveSpecification.DeviceIdColumnName}");

                entity.HasOne(dds => dds.Device)
                      .WithMany(d => d.DriveSpecifications)
                      .HasForeignKey(s => s.DeviceId)
                      .OnDelete(DeleteBehavior.Cascade)
                      .HasConstraintName($"fk_{DeviceDriveSpecification.TableName}_{Device.TableName}_{DeviceDriveSpecification.DeviceIdColumnName}");
            });
        }
    }
}
