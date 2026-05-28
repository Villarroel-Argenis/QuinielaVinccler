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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ── AppUser ──────────────────────────────────────────────────────────
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Role).HasDefaultValue(AppRoles.Common);
            entity.Property(u => u.Email).IsRequired();
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.FullName).IsRequired();
            entity.Property(u => u.CreatedAt).HasDefaultValueSql("now()");
        });

        // ── Lote ─────────────────────────────────────────────────────────────
        modelBuilder.Entity<Lote>(entity =>
        {
            entity.Property(l => l.CreatedAt).HasDefaultValueSql("now()");
        });

        // ── Planilla ─────────────────────────────────────────────────────────
        modelBuilder.Entity<Planilla>(entity =>
        {
            entity.HasIndex(p => p.Codigo).IsUnique();
            entity.Property(p => p.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(p => p.Estado).HasConversion<string>();

            entity.Ignore(p => p.IsAssigned);

            entity.HasOne(p => p.User)
                  .WithMany()
                  .HasForeignKey(p => p.UserId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(p => p.Lote)
                  .WithMany(l => l.Planillas)
                  .HasForeignKey(p => p.LoteId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(p => p.UserId);
            entity.HasIndex(p => p.LoteId);
        });

        // ── Equipo ───────────────────────────────────────────────────────────
        modelBuilder.Entity<Equipo>(entity =>
        {
            entity.HasIndex(e => e.Nombre).IsUnique();
            entity.Ignore(e => e.FlagUrl);
        });

        // ── Partido ──────────────────────────────────────────────────────────
        modelBuilder.Entity<Partido>(entity =>
        {
            entity.HasIndex(p => p.NumeroPartido).IsUnique();
            entity.HasIndex(p => p.Fase);
            entity.Property(p => p.Fase).HasConversion<string>();
            entity.Property(p => p.ResultadoGrupo).HasConversion<string>();

            entity.HasOne(p => p.EquipoLocal)
                  .WithMany()
                  .HasForeignKey(p => p.EquipoLocalId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(p => p.EquipoVisitante)
                  .WithMany()
                  .HasForeignKey(p => p.EquipoVisitanteId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(p => p.EquipoGanador)
                  .WithMany()
                  .HasForeignKey(p => p.EquipoGanadorId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // ── PrediccionGrupo ──────────────────────────────────────────────────
        modelBuilder.Entity<PrediccionGrupo>(entity =>
        {
            entity.HasIndex(p => new { p.PlanillaId, p.PartidoId }).IsUnique();
            entity.Property(p => p.ResultadoPredicho).HasConversion<string>();

            entity.HasOne(p => p.Planilla)
                  .WithMany(pl => pl.PrediccionesGrupo)
                  .HasForeignKey(p => p.PlanillaId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(p => p.Partido)
                  .WithMany()
                  .HasForeignKey(p => p.PartidoId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ── PrediccionKnockout ───────────────────────────────────────────────
        modelBuilder.Entity<PrediccionKnockout>(entity =>
        {
            entity.HasIndex(p => new { p.PlanillaId, p.PartidoId }).IsUnique();

            entity.HasOne(p => p.Planilla)
                  .WithMany(pl => pl.PrediccionesKnockout)
                  .HasForeignKey(p => p.PlanillaId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(p => p.Partido)
                  .WithMany()
                  .HasForeignKey(p => p.PartidoId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.EquipoPredichado)
                  .WithMany()
                  .HasForeignKey(p => p.EquipoPredichoId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // ── PrediccionFinal ──────────────────────────────────────────────────
        modelBuilder.Entity<PrediccionFinal>(entity =>
        {
            entity.HasIndex(p => p.PlanillaId).IsUnique();

            entity.HasOne(p => p.Planilla)
                  .WithOne(pl => pl.PrediccionFinal)
                  .HasForeignKey<PrediccionFinal>(p => p.PlanillaId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Todas las FK a Equipo con SetNull
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
    }
}