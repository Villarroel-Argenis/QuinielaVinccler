namespace QuinielaVinccler.UI.Web.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> Users { get; set; } = null!;
    public DbSet<Lote> Lotes { get; set; } = null!;
    public DbSet<Planilla> Planillas { get; set; } = null!;
    public DbSet<Equipo> Equipos { get; set; } = null!;
    public DbSet<Partido> Partidos { get; set; } = null!;
    public DbSet<PrediccionGrupo> PrediccionesGrupo { get; set; } = null!;
    public DbSet<PrediccionKnockout> PrediccionesKnockout { get; set; } = null!;
    public DbSet<PrediccionFinal> PrediccionesFinal { get; set; } = null!;
    public DbSet<ResultadoFinal> ResultadoFinal { get; set; } = null!;
    public DbSet<Configuracion> Configuraciones { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Role).HasDefaultValue(AppRoles.Common);
            entity.Property(u => u.Email).IsRequired();
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.FullName).IsRequired();
            entity.Property(u => u.CreatedAt).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<Lote>(entity =>
        {
            entity.Property(l => l.CreatedAt).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<Planilla>(entity =>
        {
            entity.HasIndex(p => p.Codigo).IsUnique();
            entity.Property(p => p.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(p => p.Estado).HasConversion<string>();
            entity.Ignore(p => p.IsAssigned);

            entity.HasOne(p => p.User).WithMany()
                  .HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(p => p.Lote).WithMany(l => l.Planillas)
                  .HasForeignKey(p => p.LoteId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(p => p.UserId);
            entity.HasIndex(p => p.LoteId);
        });

        modelBuilder.Entity<Equipo>(entity =>
        {
            entity.HasIndex(e => e.Nombre).IsUnique();
            entity.Ignore(e => e.FlagUrl);
        });

        modelBuilder.Entity<Partido>(entity =>
        {
            entity.HasIndex(p => p.NumeroPartido).IsUnique();
            entity.HasIndex(p => p.Fase);
            entity.Property(p => p.Fase).HasConversion<string>();
            entity.Property(p => p.ResultadoGrupo).HasConversion<string>();

            entity.HasOne(p => p.EquipoLocal).WithMany()
                  .HasForeignKey(p => p.EquipoLocalId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(p => p.EquipoVisitante).WithMany()
                  .HasForeignKey(p => p.EquipoVisitanteId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(p => p.EquipoGanador).WithMany()
                  .HasForeignKey(p => p.EquipoGanadorId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<PrediccionGrupo>(entity =>
        {
            entity.HasIndex(p => new { p.PlanillaId, p.PartidoId }).IsUnique();
            entity.Property(p => p.ResultadoPredicho).HasConversion<string>();

            entity.HasOne(p => p.Planilla).WithMany(pl => pl.PrediccionesGrupo)
                  .HasForeignKey(p => p.PlanillaId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(p => p.Partido).WithMany()
                  .HasForeignKey(p => p.PartidoId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PrediccionKnockout>(entity =>
        {
            entity.HasIndex(p => new { p.PlanillaId, p.PartidoId }).IsUnique();

            entity.HasOne(p => p.Planilla).WithMany(pl => pl.PrediccionesKnockout)
                  .HasForeignKey(p => p.PlanillaId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(p => p.Partido).WithMany()
                  .HasForeignKey(p => p.PartidoId).OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.EquipoLocalPredichado).WithMany()
                  .HasForeignKey(p => p.EquipoLocalPredichoId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(p => p.EquipoVisitantePredichado).WithMany()
                  .HasForeignKey(p => p.EquipoVisitantePredichoId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(p => p.EquipoGanador).WithMany()
                  .HasForeignKey(p => p.EquipoGanadorId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<PrediccionFinal>(entity =>
        {
            entity.HasIndex(p => p.PlanillaId).IsUnique();

            entity.HasOne(p => p.Planilla).WithOne(pl => pl.PrediccionFinal)
                  .HasForeignKey<PrediccionFinal>(p => p.PlanillaId).OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(p => p.Campeon).WithMany()
                  .HasForeignKey(p => p.CampeonEquipoId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(p => p.SegundoLugar).WithMany()
                  .HasForeignKey(p => p.SegundoLugarEquipoId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(p => p.TercerLugar).WithMany()
                  .HasForeignKey(p => p.TercerLugarEquipoId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(p => p.CuartoLugar).WithMany()
                  .HasForeignKey(p => p.CuartoLugarEquipoId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(p => p.MasGoleador).WithMany()
                  .HasForeignKey(p => p.MasGoleadorEquipoId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(p => p.MasGoleado).WithMany()
                  .HasForeignKey(p => p.MasGoleadoEquipoId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(p => p.MenosGoleado).WithMany()
                  .HasForeignKey(p => p.MenosGoleadoEquipoId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ResultadoFinal>(entity =>
        {
            entity.HasOne(r => r.Campeon).WithMany()
                  .HasForeignKey(r => r.CampeonEquipoId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(r => r.SegundoLugar).WithMany()
                  .HasForeignKey(r => r.SegundoLugarEquipoId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(r => r.TercerLugar).WithMany()
                  .HasForeignKey(r => r.TercerLugarEquipoId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(r => r.CuartoLugar).WithMany()
                  .HasForeignKey(r => r.CuartoLugarEquipoId).OnDelete(DeleteBehavior.SetNull);
            // MasGoleador, MasGoleado, MenosGoleado ahora son strings — sin FK
        });

        modelBuilder.Entity<Configuracion>(entity =>
        {
            entity.HasIndex(c => c.Clave).IsUnique();
            entity.Property(c => c.Clave).IsRequired();
            entity.Property(c => c.Valor).IsRequired();
        });
    }
}