using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

public class CstCaseConfiguration : IEntityTypeConfiguration<CstCase>
{
    public void Configure(EntityTypeBuilder<CstCase> builder)
    {
        builder.ToTable("cst_case");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.Macase).HasColumnName("macase").HasMaxLength(50);
        builder.Property(e => e.Matruong).HasColumnName("matruong").HasMaxLength(50);
        builder.Property(e => e.Ngaynhan).HasColumnName("ngaynhan").HasMaxLength(100);
        builder.Property(e => e.Chitietyc).HasColumnName("chitietyc");
        builder.Property(e => e.Trangthai).HasColumnName("trangthai").HasMaxLength(50);
        builder.Property(e => e.Ngaydukien).HasColumnName("ngaydukien").HasMaxLength(100);
        builder.Property(e => e.Loaihopdong).HasColumnName("loaihopdong").HasMaxLength(100);
        builder.Property(e => e.Mucdo).HasColumnName("mucdo").HasMaxLength(50);
        builder.Property(e => e.Version).HasColumnName("version").HasMaxLength(50);
        builder.Property(e => e.Hieuluc).HasColumnName("hieuluc").HasMaxLength(100);
        builder.Property(e => e.Dabangiao).HasColumnName("dabangiao");
        builder.Property(e => e.Ngayemail).HasColumnName("ngayemail").HasMaxLength(100);
        builder.Property(e => e.Mailto).HasColumnName("mailto").HasMaxLength(100);
        builder.Property(e => e.Loaicase).HasColumnName("loaicase").HasMaxLength(100);
        builder.Property(e => e.Phanhe).HasColumnName("phanhe").HasMaxLength(50);
        builder.Property(e => e.Whatnew).HasColumnName("whatnew");
        builder.Property(e => e.Teststate).HasColumnName("teststate");
        builder.Property(e => e.Thongtinkh).HasColumnName("thongtinkh");
        builder.Property(e => e.Dapungcongty).HasColumnName("dapungcongty");
        builder.Property(e => e.Comment).HasColumnName("comment");
        builder.Property(e => e.Reviewcase).HasColumnName("reviewcase");
    }
}

public class CstCase
{
    public int Id { get; set; }
    public string Macase { get; set; }
    public string Matruong { get; set; }
    public string Ngaynhan { get; set; }
    public string Chitietyc { get; set; }
    public string Trangthai { get; set; }
    public string Ngaydukien { get; set; }
    public string Loaihopdong { get; set; }
    public string Mucdo { get; set; }
    public string Version { get; set; }
    public string Hieuluc { get; set; }
    public char Dabangiao { get; set; }
    public string Ngayemail { get; set; }
    public string Mailto { get; set; }
    public string Loaicase { get; set; }
    public string Phanhe { get; set; }
    public string Whatnew { get; set; }
    public string Teststate { get; set; }
    public string Thongtinkh { get; set; }
    public string Dapungcongty { get; set; }
    public string Comment { get; set; }
    public string Reviewcase { get; set; }
}
