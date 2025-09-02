using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using api.Models;


public partial class MbkBarbell9Context : DbContext
{
    public MbkBarbell9Context()
    {
    }

    public MbkBarbell9Context(DbContextOptions<MbkBarbell9Context> options)
        : base(options)
    {
    }

    public virtual DbSet<AxleType> AxleTypes { get; set; }

    public virtual DbSet<Brass> Brasses { get; set; }

    public virtual DbSet<HstType> HstTypes { get; set; }

    public virtual DbSet<Machine> Machines { get; set; }

    public virtual DbSet<Pad> Pads { get; set; }

    public virtual DbSet<PadBrassMap> PadBrassMaps { get; set; }

    public virtual DbSet<PadHstMap> PadHstMaps { get; set; }

    public virtual DbSet<PositionType> PositionTypes { get; set; }

    public virtual DbSet<SizeRef> SizeRefs { get; set; }

    public virtual DbSet<Tool> Tools { get; set; }

    public virtual DbSet<ToolKeyAll> ToolKeyAlls { get; set; }

    public virtual DbSet<ToolKeyOriginal> ToolKeyOriginals { get; set; }

    public virtual DbSet<ToolKeyReference> ToolKeyReferences { get; set; }

    public virtual DbSet<ToolMachineMap> ToolMachineMaps { get; set; }

    public virtual DbSet<ToolPadMap> ToolPadMaps { get; set; }

    public virtual DbSet<ToolRefSpec> ToolRefSpecs { get; set; }

    public virtual DbSet<ToolSpec> ToolSpecs { get; set; }

    public virtual DbSet<TypeModel> TypeModels { get; set; }
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<Department> Departments { get; set; }
    public virtual DbSet<Role> Roles { get; set; }
    public virtual DbSet<UserStatus> UserStatuses { get; set; }
    public virtual DbSet<Log> Logs { get; set; }
    public DbSet<Request> Requests { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // #warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        //         => optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=mbk_barbell9;Trusted_Connection=True;");
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AxleType>(entity =>
        {
            entity.HasKey(e => e.axle_type_id).HasName("PK__axleType__5836A9C8D9ABCF86");

            entity.ToTable("axleTypes");

            entity.HasIndex(e => e.axle_type, "UQ__axleType__7C3600ABAFA800A6").IsUnique();

            entity.Property(e => e.axle_type_id).HasColumnName("axle_type_id");
            entity.Property(e => e.axle_type)
                .HasMaxLength(255)
                .HasColumnName("axle_type");

            // ✅ Audit fields
            entity.Property(e => e.create_by)
                .HasColumnName("create_by")
                .IsRequired();

            entity.Property(e => e.create_at)
                .HasColumnName("create_at")
                .HasColumnType("datetime2(3)")
                .HasDefaultValueSql("GETDATE()")
                .IsRequired();

            entity.Property(e => e.update_by)
                .HasColumnName("update_by");

            entity.Property(e => e.update_at)
                .HasColumnName("update_at")
                .HasColumnType("datetime2(3)");

            // ✅ Foreign Key
            entity.HasOne(e => e.CreateByUser)
                .WithMany()
                .HasForeignKey(e => e.create_by)
                .HasConstraintName("FK_axleTypes_create_by");

            entity.HasOne(e => e.UpdateByUser)
                .WithMany()
                .HasForeignKey(e => e.update_by)
                .HasConstraintName("FK_axleTypes_update_by");
        });

        modelBuilder.Entity<Brass>(entity =>
        {
            entity.HasKey(e => e.brass_id).HasName("PK__brasses__BE9E124EEA09FEDD");

            entity.ToTable("brasses");

            entity.Property(e => e.brass_id).HasColumnName("brass_id");
            entity.Property(e => e.brass_no)
                .HasMaxLength(255)
                .HasColumnName("brass_no");
            entity.Property(e => e.description)
                .HasMaxLength(255)
                .HasColumnName("description");

            // ✅ Audit fields
            entity.Property(e => e.create_by)
                .HasColumnName("create_by")
                .IsRequired();

            entity.Property(e => e.create_at)
                .HasColumnName("create_at")
                .HasColumnType("datetime2(3)")
                .HasDefaultValueSql("GETDATE()")
                .IsRequired();

            entity.Property(e => e.update_by)
                .HasColumnName("update_by");

            entity.Property(e => e.update_at)
                .HasColumnName("update_at")
                .HasColumnType("datetime2(3)");

            // ✅ Foreign Key
            entity.HasOne(e => e.CreateByUser)
                .WithMany()
                .HasForeignKey(e => e.create_by)
                .HasConstraintName("FK_brasses_create_by");

            entity.HasOne(e => e.UpdateByUser)
                .WithMany()
                .HasForeignKey(e => e.update_by)
                .HasConstraintName("FK_brasses_update_by");
        });

        modelBuilder.Entity<HstType>(entity =>
        {
            entity.HasKey(e => e.hst_type_id).HasName("PK__hstTypes__B687C4E29ACF0F22");

            entity.ToTable("hstTypes");

            entity.HasIndex(e => e.hst_type, "UQ__hstTypes__5C7FB5DEDD3E3C96").IsUnique();

            entity.Property(e => e.hst_type_id).HasColumnName("hst_type_id");
            entity.Property(e => e.hst_type)
                .HasMaxLength(255)
                .HasColumnName("hst_type");

            // ✅ Audit fields
            entity.Property(e => e.create_by)
                .HasColumnName("create_by")
                .IsRequired();

            entity.Property(e => e.create_at)
                .HasColumnName("create_at")
                .HasColumnType("datetime2(3)")
                .HasDefaultValueSql("GETDATE()")
                .IsRequired();

            entity.Property(e => e.update_by)
                .HasColumnName("update_by");

            entity.Property(e => e.update_at)
                .HasColumnName("update_at")
                .HasColumnType("datetime2(3)");

            // ✅ Foreign Key
            entity.HasOne(e => e.CreateByUser)
                .WithMany()
                .HasForeignKey(e => e.create_by)
                .HasConstraintName("FK_hstTypes_create_by");

            entity.HasOne(e => e.UpdateByUser)
                .WithMany()
                .HasForeignKey(e => e.update_by)
                .HasConstraintName("FK_hstTypes_update_by");
        });

        modelBuilder.Entity<Machine>(entity =>
        {
            entity.HasKey(e => e.machine_id).HasName("PK__machines__7B75BEA9FBEC1148");

            entity.ToTable("machines");

            entity.Property(e => e.machine_id).HasColumnName("machine_id");
            entity.Property(e => e.machine_no)
                .HasMaxLength(255)
                .HasColumnName("machine_no");

            // ✅ Audit fields
            entity.Property(e => e.create_by)
                .HasColumnName("create_by")
                .IsRequired();

            entity.Property(e => e.create_at)
                .HasColumnName("create_at")
                .HasColumnType("datetime2(3)")
                .HasDefaultValueSql("GETDATE()")
                .IsRequired();

            entity.Property(e => e.update_by)
                .HasColumnName("update_by");

            entity.Property(e => e.update_at)
                .HasColumnName("update_at")
                .HasColumnType("datetime2(3)");

            // ✅ Foreign Key
            entity.HasOne(e => e.CreateByUser)
                .WithMany()
                .HasForeignKey(e => e.create_by)
                .HasConstraintName("FK_machines_create_by");

            entity.HasOne(e => e.UpdateByUser)
                .WithMany()
                .HasForeignKey(e => e.update_by)
                .HasConstraintName("FK_machines_update_by");
        });

        modelBuilder.Entity<Pad>(entity =>
        {
            entity.HasKey(e => e.pad_id).HasName("PK__pads__2A9813960C02FD0C");

            entity.ToTable("pads");

            entity.Property(e => e.pad_id).HasColumnName("pad_id");
            entity.Property(e => e.pad_name)
                .HasMaxLength(255)
                .HasColumnName("pad_name");

            // ✅ Audit fields
            entity.Property(e => e.create_by)
                .HasColumnName("create_by")
                .IsRequired();

            entity.Property(e => e.create_at)
                .HasColumnName("create_at")
                .HasColumnType("datetime2(3)")
                .HasDefaultValueSql("GETDATE()")
                .IsRequired();

            entity.Property(e => e.update_by)
                .HasColumnName("update_by");

            entity.Property(e => e.update_at)
                .HasColumnName("update_at")
                .HasColumnType("datetime2(3)");

            // ✅ Foreign Key
            entity.HasOne(e => e.CreateByUser)
                .WithMany()
                .HasForeignKey(e => e.create_by)
                .HasConstraintName("FK_pads_create_by");

            entity.HasOne(e => e.UpdateByUser)
                .WithMany()
                .HasForeignKey(e => e.update_by)
                .HasConstraintName("FK_pads_update_by");
        });

        modelBuilder.Entity<PadBrassMap>(entity =>
        {
            entity.HasKey(e => e.pad_brass_id).HasName("PK__padBrass__B29A1BF1C2A477E9");

            entity.ToTable("padBrassMap");

            entity.Property(e => e.pad_brass_id).HasColumnName("pad_brass_id");
            entity.Property(e => e.brass_id).HasColumnName("brass_id");
            entity.Property(e => e.pad_id).HasColumnName("pad_id");

            entity.HasOne(d => d.Brass).WithMany(p => p.PadBrassMaps)
                .HasForeignKey(d => d.brass_id)
                .HasConstraintName("FK__padBrassM__brass__5FB337D6");

            entity.HasOne(d => d.Pad).WithMany(p => p.PadBrassMaps)
                .HasForeignKey(d => d.pad_id)
                .HasConstraintName("FK__padBrassM__pad_i__5EBF139D");

            // ✅ Audit fields
            entity.Property(e => e.create_by)
                .HasColumnName("create_by")
                .IsRequired();

            entity.Property(e => e.create_at)
                .HasColumnName("create_at")
                .HasColumnType("datetime2(3)")
                .HasDefaultValueSql("GETDATE()")
                .IsRequired();

            entity.Property(e => e.update_by)
                .HasColumnName("update_by");

            entity.Property(e => e.update_at)
                .HasColumnName("update_at")
                .HasColumnType("datetime2(3)");

            entity.Property(e => e.description).HasColumnName("description");


            // ✅ Foreign Key
            entity.HasOne(e => e.CreateByUser)
                .WithMany()
                .HasForeignKey(e => e.create_by)
                .HasConstraintName("FK_padBrassMap_create_by");

            entity.HasOne(e => e.UpdateByUser)
                .WithMany()
                .HasForeignKey(e => e.update_by)
                .HasConstraintName("FK_padBrassMap_update_by");

            // entity.Property(e => e.pending_request).HasColumnName("pending_request");
        });

        modelBuilder.Entity<PadHstMap>(entity =>
        {
            entity.HasKey(e => e.pad_hst_id).HasName("PK__padHstMa__6A383D81A5BBCBC6");

            entity.ToTable("padHstMap");

            entity.Property(e => e.pad_hst_id).HasColumnName("pad_hst_id");
            entity.Property(e => e.hst_type_id).HasColumnName("hst_type_id");
            entity.Property(e => e.pad_id).HasColumnName("pad_id");

            entity.HasOne(d => d.HstType).WithMany(p => p.PadHstMaps)
                .HasForeignKey(d => d.hst_type_id)
                .HasConstraintName("FK__padHstMap__hst_t__619B8048");

            entity.HasOne(d => d.Pad).WithMany(p => p.PadHstMaps)
                .HasForeignKey(d => d.pad_id)
                .HasConstraintName("FK__padHstMap__pad_i__60A75C0F");

            // ✅ Audit fields
            entity.Property(e => e.create_by)
                .HasColumnName("create_by")
                .IsRequired();

            entity.Property(e => e.create_at)
                .HasColumnName("create_at")
                .HasColumnType("datetime2(3)")
                .HasDefaultValueSql("GETDATE()")
                .IsRequired();

            entity.Property(e => e.update_by)
                .HasColumnName("update_by");

            entity.Property(e => e.update_at)
                .HasColumnName("update_at")
                .HasColumnType("datetime2(3)");

            entity.Property(e => e.description).HasColumnName("description");

            // ✅ Foreign Key
            entity.HasOne(e => e.CreateByUser)
                .WithMany()
                .HasForeignKey(e => e.create_by)
                .HasConstraintName("FK_padHstMap_create_by");

            entity.HasOne(e => e.UpdateByUser)
                .WithMany()
                .HasForeignKey(e => e.update_by)
                .HasConstraintName("FK_padHstMap_update_by");
        });

        modelBuilder.Entity<PositionType>(entity =>
        {
            entity.HasKey(e => e.position_type_id).HasName("PK__position__980F865EABF8959D");

            entity.ToTable("positionTypes");

            entity.HasIndex(e => e.position_type, "UQ__position__18CE58105224A102").IsUnique();

            entity.Property(e => e.position_type_id).HasColumnName("position_type_id");
            entity.Property(e => e.position_type)
                .HasMaxLength(255)
                .HasColumnName("position_type");

            // ✅ Audit fields
            entity.Property(e => e.create_by)
                .HasColumnName("create_by")
                .IsRequired();

            entity.Property(e => e.create_at)
                .HasColumnName("create_at")
                .HasColumnType("datetime2(3)")
                .HasDefaultValueSql("GETDATE()")
                .IsRequired();

            entity.Property(e => e.update_by)
                .HasColumnName("update_by");

            entity.Property(e => e.update_at)
                .HasColumnName("update_at")
                .HasColumnType("datetime2(3)");

            // ✅ Foreign Key
            entity.HasOne(e => e.CreateByUser)
                .WithMany()
                .HasForeignKey(e => e.create_by)
                .HasConstraintName("FK_positionTypes_create_by");

            entity.HasOne(e => e.UpdateByUser)
                .WithMany()
                .HasForeignKey(e => e.update_by)
                .HasConstraintName("FK_positionTypes_update_by");
        });

        modelBuilder.Entity<SizeRef>(entity =>
        {
            entity.HasKey(e => e.size_ref_id).HasName("PK__sizeRefs__5EB20B267941E6F5");

            entity.ToTable("sizeRefs");

            entity.HasIndex(e => e.size_ref, "UQ__sizeRefs__0111F1F62F31D25F").IsUnique();

            entity.Property(e => e.size_ref_id).HasColumnName("size_ref_id");
            entity.Property(e => e.size_ref)
                .HasMaxLength(255)
                .HasColumnName("size_ref");

            // ✅ Audit fields
            entity.Property(e => e.create_by)
                .HasColumnName("create_by")
                .IsRequired();

            entity.Property(e => e.create_at)
                .HasColumnName("create_at")
                .HasColumnType("datetime2(3)")
                .HasDefaultValueSql("GETDATE()")
                .IsRequired();

            entity.Property(e => e.update_by)
                .HasColumnName("update_by");

            entity.Property(e => e.update_at)
                .HasColumnName("update_at")
                .HasColumnType("datetime2(3)");

            // ✅ Foreign Key
            entity.HasOne(e => e.CreateByUser)
                .WithMany()
                .HasForeignKey(e => e.create_by)
                .HasConstraintName("FK_sizeRefs_create_by");

            entity.HasOne(e => e.UpdateByUser)
                .WithMany()
                .HasForeignKey(e => e.update_by)
                .HasConstraintName("FK_sizeRefs_update_by");
        });

        modelBuilder.Entity<Tool>(entity =>
        {
            entity.HasKey(e => e.tool_id).HasName("PK__tools__28DE264F5733D1C9");

            entity.ToTable("tools");

            entity.Property(e => e.tool_id).HasColumnName("tool_id");

            entity.Property(e => e.tool_name)
                .HasMaxLength(255)
                .HasColumnName("tool_name");

            // ✅ Audit fields
            entity.Property(e => e.create_by)
                .HasColumnName("create_by")
                .IsRequired();

            entity.Property(e => e.create_at)
                .HasColumnName("create_at")
                .HasColumnType("datetime2(3)")
                .HasDefaultValueSql("GETDATE()")
                .IsRequired();

            entity.Property(e => e.update_by)
                .HasColumnName("update_by");

            entity.Property(e => e.update_at)
                .HasColumnName("update_at")
                .HasColumnType("datetime2(3)");

            // ✅ Foreign Key
            entity.HasOne(e => e.CreateByUser)
                .WithMany()
                .HasForeignKey(e => e.create_by)
                .HasConstraintName("FK_tools_create_by");

            entity.HasOne(e => e.UpdateByUser)
                .WithMany()
                .HasForeignKey(e => e.update_by)
                .HasConstraintName("FK_tools_update_by");
        });


        modelBuilder.Entity<ToolKeyAll>(entity =>
        {
            entity.HasKey(e => e.tool_key_id).HasName("PK__toolKeyA__0F5ABAEA532249B9");

            entity.ToTable("toolKeyAlls");

            entity.Property(e => e.tool_key_id).HasColumnName("tool_key_id");
            entity.Property(e => e.original_spec).HasColumnName("original_spec");
            entity.Property(e => e.size_ref_id).HasColumnName("size_ref_id");
            entity.Property(e => e.source_original_key_id).HasColumnName("source_original_key_id");
            entity.Property(e => e.source_ref_key_id).HasColumnName("source_ref_key_id");
            entity.Property(e => e.tool_id).HasColumnName("tool_id");
            entity.Property(e => e.tool_ref_id).HasColumnName("tool_ref_id");
            entity.Property(e => e.type_id).HasColumnName("type_id");
            entity.Property(e => e.type_ref_id).HasColumnName("type_ref_id");
            entity.Property(e => e.ref_spec).HasColumnName("ref_spec");
            entity.Property(e => e.knurling).HasColumnName("knurling");

            entity.HasOne(d => d.SizeRef).WithMany(p => p.ToolKeyAlls)
                .HasForeignKey(d => d.size_ref_id)
                .HasConstraintName("FK__toolKeyAl__size___5BE2A6F2");

            entity.HasOne(d => d.SourceOriginalKey).WithMany(p => p.ToolKeyAlls)
                .HasForeignKey(d => d.source_original_key_id)
                .HasConstraintName("FK__toolKeyAl__sourc__5CD6CB2B");

            entity.HasOne(d => d.SourceRefKey).WithMany(p => p.ToolKeyAlls)
                .HasForeignKey(d => d.source_ref_key_id)
                .HasConstraintName("FK__toolKeyAl__sourc__5DCAEF64");

            // // ✅ ref_spec
            // entity.Property(e => e.ref_spec)
            //     .HasColumnName("ref_spec")
            //     .HasColumnType("int");
            // // ✅ knurling
            // entity.Property(e => e.knurling)
            //     .HasColumnName("knurling")
            //     .HasColumnType("int");

            entity.HasOne(d => d.Tool).WithMany(p => p.ToolKeyAllTools)
                .HasForeignKey(d => d.tool_id)
                .HasConstraintName("FK__toolKeyAl__tool___59063A47");

            entity.HasOne(d => d.ToolRef).WithMany(p => p.ToolKeyAllToolRefs)
                .HasForeignKey(d => d.tool_ref_id)
                .HasConstraintName("FK__toolKeyAl__tool___5AEE82B9");

            entity.HasOne(d => d.Type).WithMany(p => p.ToolKeyAllTypes)
                .HasForeignKey(d => d.type_id)
                .HasConstraintName("FK__toolKeyAl__type___5812160E");

            entity.HasOne(d => d.TypeRef).WithMany(p => p.ToolKeyAllTypeRefs)
                .HasForeignKey(d => d.type_ref_id)
                .HasConstraintName("FK__toolKeyAl__type___59FA5E80");
        });

        modelBuilder.Entity<ToolKeyOriginal>(entity =>
        {
            entity.HasKey(e => e.tool_key_id).HasName("PK__toolKeyO__0F5ABAEA21475086");

            entity.ToTable("toolKeyOriginals");

            entity.Property(e => e.tool_key_id).HasColumnName("tool_key_id");
            entity.Property(e => e.knurling_type).HasColumnName("knurling_type");
            entity.Property(e => e.size_ref_id).HasColumnName("size_ref_id");
            entity.Property(e => e.tool_id).HasColumnName("tool_id");
            entity.Property(e => e.type_id).HasColumnName("type_id");

            entity.HasOne(d => d.SizeRef).WithMany(p => p.ToolKeyOriginals)
                .HasForeignKey(d => d.size_ref_id)
                .HasConstraintName("FK__toolKeyOr__size___4BAC3F29");

            entity.HasOne(d => d.Tool).WithMany(p => p.ToolKeyOriginals)
                .HasForeignKey(d => d.tool_id)
                .HasConstraintName("FK__toolKeyOr__tool___49C3F6B7");

            entity.HasOne(d => d.Type).WithMany(p => p.ToolKeyOriginals)
                .HasForeignKey(d => d.type_id)
                .HasConstraintName("FK__toolKeyOr__type___4AB81AF0");

            // ✅ Audit fields
            entity.Property(e => e.create_by)
                .HasColumnName("create_by")
                .IsRequired();

            entity.Property(e => e.create_at)
                .HasColumnName("create_at")
                .HasColumnType("datetime2(3)")
                .HasDefaultValueSql("GETDATE()")
                .IsRequired();

            entity.Property(e => e.update_by)
                .HasColumnName("update_by");

            entity.Property(e => e.update_at)
                .HasColumnName("update_at")
                .HasColumnType("datetime2(3)");

            // ✅ Foreign Key
            entity.HasOne(e => e.CreateByUser)
                .WithMany()
                .HasForeignKey(e => e.create_by)
                .HasConstraintName("FK_toolKeyOriginals_create_by");

            entity.HasOne(e => e.UpdateByUser)
                .WithMany()
                .HasForeignKey(e => e.update_by)
                .HasConstraintName("FK_toolKeyOriginals_update_by");
        });

        modelBuilder.Entity<ToolKeyReference>(entity =>
        {
            entity.HasKey(e => e.ref_key_id).HasName("PK__toolKeyR__5A93C44C8BB949B3");

            entity.ToTable("toolKeyReferences");

            entity.Property(e => e.ref_key_id).HasColumnName("ref_key_id");
            entity.Property(e => e.knurling_type).HasColumnName("knurling_type");
            entity.Property(e => e.position_type_id).HasColumnName("position_type_id");
            entity.Property(e => e.tool_id).HasColumnName("tool_id");
            entity.Property(e => e.tool_key_id).HasColumnName("tool_key_id");
            entity.Property(e => e.type_id).HasColumnName("type_id");

            entity.HasOne(d => d.PositionType).WithMany(p => p.ToolKeyReferences)
                .HasForeignKey(d => d.position_type_id)
                .HasConstraintName("FK__toolKeyRe__posit__4E88ABD4");

            entity.HasOne(d => d.Tool).WithMany(p => p.ToolKeyReferences)
                .HasForeignKey(d => d.tool_id)
                .HasConstraintName("FK__toolKeyRe__tool___4CA06362");

            entity.HasOne(d => d.ToolKey).WithMany(p => p.ToolKeyReferences)
                .HasForeignKey(d => d.tool_key_id)
                .HasConstraintName("FK__toolKeyRe__tool___4F7CD00D");

            entity.HasOne(d => d.Type).WithMany(p => p.ToolKeyReferences)
                .HasForeignKey(d => d.type_id)
                .HasConstraintName("FK__toolKeyRe__type___4D94879B");

            // ✅ Audit fields
            entity.Property(e => e.create_by)
                .HasColumnName("create_by")
                .IsRequired();

            entity.Property(e => e.create_at)
                .HasColumnName("create_at")
                .HasColumnType("datetime2(3)")
                .HasDefaultValueSql("GETDATE()")
                .IsRequired();

            entity.Property(e => e.update_by)
                .HasColumnName("update_by");

            entity.Property(e => e.update_at)
                .HasColumnName("update_at")
                .HasColumnType("datetime2(3)");

            // ✅ Foreign Key
            entity.HasOne(e => e.CreateByUser)
                .WithMany()
                .HasForeignKey(e => e.create_by)
                .HasConstraintName("FK_toolKeyReferences_create_by");

            entity.HasOne(e => e.UpdateByUser)
                .WithMany()
                .HasForeignKey(e => e.update_by)
                .HasConstraintName("FK_toolKeyReferences_update_by");
        });

        modelBuilder.Entity<ToolMachineMap>(entity =>
        {
            entity.HasKey(e => e.map_id).HasName("PK__toolMach__52A7881916650358");

            entity.ToTable("toolMachineMap");

            entity.Property(e => e.map_id).HasColumnName("map_id");
            entity.Property(e => e.machine_id).HasColumnName("machine_id");
            entity.Property(e => e.tool_key_id).HasColumnName("tool_key_id");

            entity.HasOne(d => d.Machine).WithMany(p => p.ToolMachineMaps)
                .HasForeignKey(d => d.machine_id)
                .HasConstraintName("FK__toolMachi__machi__571DF1D5");

            entity.HasOne(d => d.ToolKey).WithMany(p => p.ToolMachineMaps)
                .HasForeignKey(d => d.tool_key_id)
                .HasConstraintName("FK__toolMachi__tool___5629CD9C");

            
            entity.Property(e => e.description).HasColumnName("description");
            // ✅ Audit fields
            entity.Property(e => e.create_by)
                .HasColumnName("create_by")
                .IsRequired();

            entity.Property(e => e.create_at)
                .HasColumnName("create_at")
                .HasColumnType("datetime2(3)")
                .HasDefaultValueSql("GETDATE()")
                .IsRequired();

            entity.Property(e => e.update_by)
                .HasColumnName("update_by");

            entity.Property(e => e.update_at)
                .HasColumnName("update_at")
                .HasColumnType("datetime2(3)");

            // ✅ Foreign Key
            entity.HasOne(e => e.CreateByUser)
                .WithMany()
                .HasForeignKey(e => e.create_by)
                .HasConstraintName("FK_toolMachineMap_create_by");

            entity.HasOne(e => e.UpdateByUser)
                .WithMany()
                .HasForeignKey(e => e.update_by)
                .HasConstraintName("FK_toolMachineMap_update_by");
        });

        modelBuilder.Entity<ToolPadMap>(entity =>
        {
            entity.HasKey(e => e.map_id).HasName("PK__toolPadM__52A788192C5542F1");

            entity.ToTable("toolPadMap");

            entity.Property(e => e.map_id).HasColumnName("map_id");
            entity.Property(e => e.hst_type_id).HasColumnName("hst_type_id");
            entity.Property(e => e.pad_id).HasColumnName("pad_id");
            entity.Property(e => e.tool_key_id).HasColumnName("tool_key_id");

            entity.HasOne(d => d.HstType).WithMany(p => p.ToolPadMaps)
                .HasForeignKey(d => d.hst_type_id)
                .HasConstraintName("FK__toolPadMa__hst_t__6477ECF3");

            entity.HasOne(d => d.Pad).WithMany(p => p.ToolPadMaps)
                .HasForeignKey(d => d.pad_id)
                .HasConstraintName("FK__toolPadMa__pad_i__6383C8BA");

            entity.HasOne(d => d.ToolKey).WithMany(p => p.ToolPadMaps)
                .HasForeignKey(d => d.tool_key_id)
                .HasConstraintName("FK__toolPadMa__tool___628FA481");

            entity.Property(e => e.description).HasColumnName("description");

            // ✅ Audit fields
            entity.Property(e => e.create_by)
                .HasColumnName("create_by")
                .IsRequired();

            entity.Property(e => e.create_at)
                .HasColumnName("create_at")
                .HasColumnType("datetime2(3)")
                .HasDefaultValueSql("GETDATE()")
                .IsRequired();

            entity.Property(e => e.update_by)
                .HasColumnName("update_by");

            entity.Property(e => e.update_at)
                .HasColumnName("update_at")
                .HasColumnType("datetime2(3)");

            // ✅ Foreign Key
            entity.HasOne(e => e.CreateByUser)
                .WithMany()
                .HasForeignKey(e => e.create_by)
                .HasConstraintName("FK_toolPadMap_create_by");

            entity.HasOne(e => e.UpdateByUser)
                .WithMany()
                .HasForeignKey(e => e.update_by)
                .HasConstraintName("FK_toolPadMap_update_by");
        });

        modelBuilder.Entity<ToolRefSpec>(entity =>
        {
            entity.HasKey(e => e.tool_ref_spec_id).HasName("PK__toolRefS__3787486C1C81B5B8");

            entity.ToTable("toolRefSpecs");

            entity.Property(e => e.tool_ref_spec_id).HasColumnName("tool_ref_spec_id");
            entity.Property(e => e.axle_type_id).HasColumnName("axle_type_id");
            entity.Property(e => e.b2b_max).HasColumnName("b2b_max");
            entity.Property(e => e.b2b_min).HasColumnName("b2b_min");
            entity.Property(e => e.chassis_span)
                .HasMaxLength(255)
                .HasColumnName("chassis_span");
            entity.Property(e => e.chassis_span1).HasColumnName("chassis_span1");
            entity.Property(e => e.chassis_span2).HasColumnName("chassis_span2");
            entity.Property(e => e.f_shank_max).HasColumnName("f_shank_max");
            entity.Property(e => e.f_shank_min).HasColumnName("f_shank_min");
            entity.Property(e => e.h2h_max).HasColumnName("h2h_max");
            entity.Property(e => e.h2h_min).HasColumnName("h2h_min");
            entity.Property(e => e.is_original_spec).HasColumnName("is_original_spec");
            entity.Property(e => e.knurling_type).HasColumnName("knurling_type");
            entity.Property(e => e.overall_a).HasColumnName("overall_a");
            entity.Property(e => e.overall_b).HasColumnName("overall_b");
            entity.Property(e => e.overall_c).HasColumnName("overall_c");
            entity.Property(e => e.source)
                .HasMaxLength(255)
                .HasColumnName("source");
            entity.Property(e => e.description).HasColumnName("description");

            entity.Property(e => e.tolerance_a).HasColumnName("tolerance_a");
            entity.Property(e => e.tolerance_b).HasColumnName("tolerance_b");
            entity.Property(e => e.tolerance_c).HasColumnName("tolerance_c");
            entity.Property(e => e.tool_key_id).HasColumnName("tool_key_id");

            entity.HasOne(d => d.AxleType).WithMany(p => p.ToolRefSpecs)
                .HasForeignKey(d => d.axle_type_id)
                .HasConstraintName("FK__toolRefSp__axle___5165187F");

            entity.HasOne(d => d.ToolKey).WithMany(p => p.ToolRefSpecs)
                .HasForeignKey(d => d.tool_key_id)
                .HasConstraintName("FK__toolRefSp__tool___5070F446");

            // ✅ Audit fields
            entity.Property(e => e.create_by)
                .HasColumnName("create_by")
                .IsRequired();

            entity.Property(e => e.create_at)
                .HasColumnName("create_at")
                .HasColumnType("datetime2(3)")
                .HasDefaultValueSql("GETDATE()")
                .IsRequired();

            entity.Property(e => e.update_by)
                .HasColumnName("update_by");

            entity.Property(e => e.update_at)
                .HasColumnName("update_at")
                .HasColumnType("datetime2(3)");

            // ✅ Foreign Key
            entity.HasOne(e => e.CreateByUser)
                .WithMany()
                .HasForeignKey(e => e.create_by)
                .HasConstraintName("FK_toolRefSpecs_create_by");

            entity.HasOne(e => e.UpdateByUser)
                .WithMany()
                .HasForeignKey(e => e.update_by)
                .HasConstraintName("FK_toolRefSpecs_update_by");
        });

        modelBuilder.Entity<ToolSpec>(entity =>
        {
            entity.HasKey(e => e.tool_spec_id).HasName("PK__toolSpec__C4F84CD357875A7D");

            entity.ToTable("toolSpecs");

            entity.Property(e => e.tool_spec_id).HasColumnName("tool_spec_id");
            entity.Property(e => e.axle_type_id).HasColumnName("axle_type_id");
            entity.Property(e => e.chassis_span_override).HasColumnName("chassis_span_override");
            entity.Property(e => e.description).HasColumnName("description");
            entity.Property(e => e.position_type_id).HasColumnName("position_type_id");
            entity.Property(e => e.ref_key_id).HasColumnName("ref_key_id");
            entity.Property(e => e.tool_ref_spec_id).HasColumnName("tool_ref_spec_id");

            entity.HasOne(d => d.AxleType).WithMany(p => p.ToolSpecs)
                .HasForeignKey(d => d.axle_type_id)
                .HasConstraintName("FK__toolSpecs__axle___5535A963");

            entity.HasOne(d => d.PositionType).WithMany(p => p.ToolSpecs)
                .HasForeignKey(d => d.position_type_id)
                .HasConstraintName("FK__toolSpecs__posit__5441852A");

            entity.HasOne(d => d.RefKey).WithMany(p => p.ToolSpecs)
                .HasForeignKey(d => d.ref_key_id)
                .HasConstraintName("FK__toolSpecs__ref_k__52593CB8");

            entity.HasOne(d => d.ToolRefSpec).WithMany(p => p.ToolSpecs)
                .HasForeignKey(d => d.tool_ref_spec_id)
                .HasConstraintName("FK__toolSpecs__tool___534D60F1");

            // ✅ Audit fields
            entity.Property(e => e.create_by)
                .HasColumnName("create_by")
                .IsRequired();

            entity.Property(e => e.create_at)
                .HasColumnName("create_at")
                .HasColumnType("datetime2(3)")
                .HasDefaultValueSql("GETDATE()")
                .IsRequired();

            entity.Property(e => e.update_by)
                .HasColumnName("update_by");

            entity.Property(e => e.update_at)
                .HasColumnName("update_at")
                .HasColumnType("datetime2(3)");

            // ✅ Foreign Key
            entity.HasOne(e => e.CreateByUser)
                .WithMany()
                .HasForeignKey(e => e.create_by)
                .HasConstraintName("FK_toolSpecs_create_by");

            entity.HasOne(e => e.UpdateByUser)
                .WithMany()
                .HasForeignKey(e => e.update_by)
                .HasConstraintName("FK_toolSpecs_update_by");
        });

        modelBuilder.Entity<TypeModel>(entity =>
        {
            entity.HasKey(e => e.type_id).HasName("PK__typeMode__2C000598A6A2F76A");

            entity.ToTable("typeModels");

            entity.HasIndex(e => e.type_name, "UQ__typeMode__543C4FD94A57B0EC").IsUnique();

            entity.Property(e => e.type_id).HasColumnName("type_id");
            entity.Property(e => e.type_name)
                .HasMaxLength(255)
                .HasColumnName("type_name");

            // ✅ Audit fields
            entity.Property(e => e.create_by)
                .HasColumnName("create_by")
                .IsRequired();

            entity.Property(e => e.create_at)
                .HasColumnName("create_at")
                .HasColumnType("datetime2(3)")
                .HasDefaultValueSql("GETDATE()")
                .IsRequired();

            entity.Property(e => e.update_by)
                .HasColumnName("update_by");

            entity.Property(e => e.update_at)
                .HasColumnName("update_at")
                .HasColumnType("datetime2(3)");

            // ✅ Foreign Key
            entity.HasOne(e => e.CreateByUser)
                .WithMany()
                .HasForeignKey(e => e.create_by)
                .HasConstraintName("FK_typeModels_create_by");

            entity.HasOne(e => e.UpdateByUser)
                .WithMany()
                .HasForeignKey(e => e.update_by)
                .HasConstraintName("FK_typeModels_update_by");
        });


        modelBuilder.Entity<User>(entity =>
    {
        entity.ToTable("users");

        entity.HasKey(e => e.user_id);

        entity.Property(e => e.user_id).HasColumnName("user_id");
        // entity.Property(e => e.username).HasColumnName("username").HasMaxLength(255).IsRequired();
        entity.Property(e => e.password_hash).HasColumnName("password_hash").IsRequired();
        entity.Property(e => e.first_name).HasColumnName("first_name").HasMaxLength(255).IsRequired();
        entity.Property(e => e.last_name).HasColumnName("last_name").HasMaxLength(255).IsRequired();
        entity.Property(e => e.department_id).HasColumnName("department_id");
        entity.Property(e => e.role_id).HasColumnName("role_id");
        entity.Property(e => e.status_id).HasColumnName("status_id");
        entity.Property(e => e.employee_id).HasColumnName("employee_id").IsRequired();
        entity.Property(e => e.email)
            .HasMaxLength(255)
            .IsRequired(false);

        entity.HasOne(d => d.Department)
            .WithMany(p => p.users)
            .HasForeignKey(d => d.department_id)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(d => d.Role)
            .WithMany(p => p.users)
            .HasForeignKey(d => d.role_id)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(d => d.UserStatus)
            .WithMany(p => p.users)
            .HasForeignKey(d => d.status_id)
            .OnDelete(DeleteBehavior.Restrict);

    });

        modelBuilder.Entity<Department>(entity =>
    {
        entity.ToTable("departments");

        entity.HasKey(e => e.department_id);

        entity.Property(e => e.department_id).HasColumnName("department_id");
        entity.Property(e => e.department_name).HasColumnName("department_name").HasMaxLength(255).IsRequired();
    });

        modelBuilder.Entity<Role>(entity =>
    {
        entity.ToTable("roles");

        entity.HasKey(e => e.role_id);

        entity.Property(e => e.role_id).HasColumnName("role_id");
        entity.Property(e => e.role_name).HasColumnName("role_name").HasMaxLength(255).IsRequired();
    });

        modelBuilder.Entity<UserStatus>(entity =>
        {
            entity.ToTable("user_statuses");

            entity.HasKey(e => e.status_id);

            entity.Property(e => e.status_id).HasColumnName("status_id");
            entity.Property(e => e.status_name).HasColumnName("status_name").HasMaxLength(255).IsRequired();
        });

        // ===== Logs =====
        modelBuilder.Entity<Log>(entity =>
        {
            entity.ToTable("logs");

            entity.HasKey(e => e.log_id);

            entity.Property(e => e.log_id)
                .HasColumnName("log_id")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.user_id)
                .HasColumnName("user_id");

            entity.Property(e => e.username_snapshot)
                .HasMaxLength(255)
                .HasColumnName("username_snapshot");

            entity.Property(e => e.action)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("action");

            entity.Property(e => e.target_table)
                .HasMaxLength(100)
                .HasColumnName("target_table");

            entity.Property(e => e.target_id)
                .HasMaxLength(255)
                .HasColumnName("target_id");

            // entity.Property(e => e.details)
            //     .HasColumnName("details");

            entity.Property(e => e.old_data)
                .HasColumnName("old_data");

            entity.Property(e => e.new_data)
                .HasColumnName("new_data");

            // entity.Property(e => e.ip_address)
            //     .HasMaxLength(50)
            //     .HasColumnName("ip_address");

            // entity.Property(e => e.device)
            //     .HasMaxLength(255)
            //     .HasColumnName("device");

            // entity.Property(e => e.os_info)
            //     .HasMaxLength(255)
            //     .HasColumnName("os_info");

            // entity.Property(e => e.endpoint_url)
            //     .HasMaxLength(1024)
            //     .HasColumnName("endpoint_url");

            // entity.Property(e => e.http_method)
            //     .HasMaxLength(10)
            //     .HasColumnName("http_method");

            // entity.Property(e => e.response_status)
            //     .HasColumnName("response_status");

            entity.Property(e => e.created_at)
                .HasColumnName("created_at")
                .HasColumnType("datetime2(3)")
                .HasDefaultValueSql("GETDATE()");

            // FK
            entity.HasOne<User>()        // ถ้าคุณมี DbSet<User>
                .WithMany()
                .HasForeignKey(e => e.user_id)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Request>(entity =>
        {
            entity.ToTable("Requests");

            entity.HasKey(e => e.request_id);

            entity.Property(e => e.request_type)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.request_status)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.target_table)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.target_pk_id)
                .IsRequired(false);

            entity.Property(e => e.old_data)
                .HasColumnType("nvarchar(max)");

            entity.Property(e => e.new_data)
                .HasColumnType("nvarchar(max)");

            entity.Property(e => e.requested_by)
                .IsRequired();

            entity.Property(e => e.requested_at)
                .HasColumnType("datetime")
                .IsRequired();

            entity.Property(e => e.approved_by)
                .IsRequired(false);

            entity.Property(e => e.approved_at)
                .HasColumnType("datetime")
                .IsRequired(false);

            entity.Property(e => e.note)
                .HasMaxLength(500)
                .IsRequired(false);

            // FK → Users (requested_by)
            entity.HasOne(d => d.RequestedByUser)
                .WithMany()
                .HasForeignKey(d => d.requested_by)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Requests_RequestedBy");

            // FK → Users (approved_by)
            entity.HasOne(d => d.ApprovedByUser)
                .WithMany()
                .HasForeignKey(d => d.approved_by)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Requests_ApprovedBy");
        });



        OnModelCreatingPartial(modelBuilder);
    }



    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
