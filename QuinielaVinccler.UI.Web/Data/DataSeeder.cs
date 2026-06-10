namespace QuinielaVinccler.UI.Web.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext db, IConfiguration config)
    {
        await db.Database.MigrateAsync();
        await SeedAdminAsync(db, config);
        await SeedConfiguracionAsync(db, config);
        await SeedEquiposAsync(db);
        await SeedPartidosAsync(db);
    }

    // ── Admin ────────────────────────────────────────────────────────────────
    private static async Task SeedAdminAsync(AppDbContext db, IConfiguration config)
    {
        var email = config["Seed:AdminEmail"] ?? "admin@quinielavinccler.com";
        var password = config["Seed:AdminPassword"] ?? "admin2026";

        if (await db.Users.AnyAsync(u => u.Email == email)) return;

        db.Users.Add(new AppUser
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            FullName = "Administrador",
            CI = "00000000",
            Telefono = "00000000000",
            Role = AppRoles.Admin
        });

        await db.SaveChangesAsync();
    }

    // ── Configuracion ────────────────────────────────────────────────────────
    private static async Task SeedConfiguracionAsync(AppDbContext db, IConfiguration config)
    {
        var fechaCierre = config["Quiniela:FechaCierreUtc"] ?? "2026-06-11T23:59:59Z";

        var claves = new[]
        {
            (ConfiguracionKeys.QuinielaCerrada, "false"),
            (ConfiguracionKeys.FechaCierreUtc,  fechaCierre),
            (ConfiguracionKeys.PermitirIncompletasEnRanking, "false")
        };

        foreach (var (clave, valorDefault) in claves)
        {
            if (!await db.Configuraciones.AnyAsync(c => c.Clave == clave))
            {
                db.Configuraciones.Add(new Configuracion { Clave = clave, Valor = valorDefault });
            }
        }

        await db.SaveChangesAsync();
    }

    // ── Equipos ──────────────────────────────────────────────────────────────
    private static async Task SeedEquiposAsync(AppDbContext db)
    {
        if (await db.Equipos.AnyAsync()) return;

        var equipos = new List<Equipo>
        {
            new() { Nombre = "México",         CodigoIso = "mx",     Grupo = "A" },
            new() { Nombre = "Sudáfrica",       CodigoIso = "za",     Grupo = "A" },
            new() { Nombre = "Corea del Sur",   CodigoIso = "kr",     Grupo = "A" },
            new() { Nombre = "Rep. Checa",      CodigoIso = "cz",     Grupo = "A" },
            new() { Nombre = "Canadá",          CodigoIso = "ca",     Grupo = "B" },
            new() { Nombre = "Bosnia y Herz.",  CodigoIso = "ba",     Grupo = "B" },
            new() { Nombre = "Catar",           CodigoIso = "qa",     Grupo = "B" },
            new() { Nombre = "Suiza",           CodigoIso = "ch",     Grupo = "B" },
            new() { Nombre = "Brasil",          CodigoIso = "br",     Grupo = "C" },
            new() { Nombre = "Marruecos",       CodigoIso = "ma",     Grupo = "C" },
            new() { Nombre = "Haití",           CodigoIso = "ht",     Grupo = "C" },
            new() { Nombre = "Escocia",         CodigoIso = "gb-sct", Grupo = "C" },
            new() { Nombre = "EEUU",            CodigoIso = "us",     Grupo = "D" },
            new() { Nombre = "Paraguay",        CodigoIso = "py",     Grupo = "D" },
            new() { Nombre = "Australia",       CodigoIso = "au",     Grupo = "D" },
            new() { Nombre = "Turquía",         CodigoIso = "tr",     Grupo = "D" },
            new() { Nombre = "Alemania",        CodigoIso = "de",     Grupo = "E" },
            new() { Nombre = "Curazao",         CodigoIso = "cw",     Grupo = "E" },
            new() { Nombre = "Costa de Marfil", CodigoIso = "ci",     Grupo = "E" },
            new() { Nombre = "Ecuador",         CodigoIso = "ec",     Grupo = "E" },
            new() { Nombre = "Países Bajos",    CodigoIso = "nl",     Grupo = "F" },
            new() { Nombre = "Japón",           CodigoIso = "jp",     Grupo = "F" },
            new() { Nombre = "Suecia",          CodigoIso = "se",     Grupo = "F" },
            new() { Nombre = "Túnez",           CodigoIso = "tn",     Grupo = "F" },
            new() { Nombre = "Bélgica",         CodigoIso = "be",     Grupo = "G" },
            new() { Nombre = "Egipto",          CodigoIso = "eg",     Grupo = "G" },
            new() { Nombre = "Irán",            CodigoIso = "ir",     Grupo = "G" },
            new() { Nombre = "Nueva Zelanda",   CodigoIso = "nz",     Grupo = "G" },
            new() { Nombre = "España",          CodigoIso = "es",     Grupo = "H" },
            new() { Nombre = "Cabo Verde",      CodigoIso = "cv",     Grupo = "H" },
            new() { Nombre = "Arabia Saudita",  CodigoIso = "sa",     Grupo = "H" },
            new() { Nombre = "Uruguay",         CodigoIso = "uy",     Grupo = "H" },
            new() { Nombre = "Francia",         CodigoIso = "fr",     Grupo = "I" },
            new() { Nombre = "Senegal",         CodigoIso = "sn",     Grupo = "I" },
            new() { Nombre = "Irak",            CodigoIso = "iq",     Grupo = "I" },
            new() { Nombre = "Noruega",         CodigoIso = "no",     Grupo = "I" },
            new() { Nombre = "Argentina",       CodigoIso = "ar",     Grupo = "J" },
            new() { Nombre = "Argelia",         CodigoIso = "dz",     Grupo = "J" },
            new() { Nombre = "Austria",         CodigoIso = "at",     Grupo = "J" },
            new() { Nombre = "Jordania",        CodigoIso = "jo",     Grupo = "J" },
            new() { Nombre = "Portugal",        CodigoIso = "pt",     Grupo = "K" },
            new() { Nombre = "RD Congo",        CodigoIso = "cd",     Grupo = "K" },
            new() { Nombre = "Uzbekistán",      CodigoIso = "uz",     Grupo = "K" },
            new() { Nombre = "Colombia",        CodigoIso = "co",     Grupo = "K" },
            new() { Nombre = "Inglaterra",      CodigoIso = "gb-eng", Grupo = "L" },
            new() { Nombre = "Croacia",         CodigoIso = "hr",     Grupo = "L" },
            new() { Nombre = "Ghana",           CodigoIso = "gh",     Grupo = "L" },
            new() { Nombre = "Panamá",          CodigoIso = "pa",     Grupo = "L" },
        };

        db.Equipos.AddRange(equipos);
        await db.SaveChangesAsync();
    }

    // ── Partidos ─────────────────────────────────────────────────────────────
    private static async Task SeedPartidosAsync(AppDbContext db)
    {
        if (await db.Partidos.AnyAsync()) return;

        var equipoDict = await db.Equipos.ToDictionaryAsync(e => e.Nombre, e => e.Id);
        var partidos = new List<Partido>();
        int numero = 1;

        var grupos = new[]
        {
            ("A", new[] { "México",       "Sudáfrica",      "Corea del Sur",   "Rep. Checa"    }),
            ("B", new[] { "Canadá",       "Bosnia y Herz.", "Catar",           "Suiza"         }),
            ("C", new[] { "Brasil",       "Marruecos",      "Haití",           "Escocia"       }),
            ("D", new[] { "EEUU",         "Paraguay",       "Australia",       "Turquía"       }),
            ("E", new[] { "Alemania",     "Curazao",        "Costa de Marfil", "Ecuador"       }),
            ("F", new[] { "Países Bajos", "Japón",          "Suecia",          "Túnez"         }),
            ("G", new[] { "Bélgica",      "Egipto",         "Irán",            "Nueva Zelanda" }),
            ("H", new[] { "España",       "Cabo Verde",     "Arabia Saudita",  "Uruguay"       }),
            ("I", new[] { "Francia",      "Senegal",        "Irak",            "Noruega"       }),
            ("J", new[] { "Argentina",    "Argelia",        "Austria",         "Jordania"      }),
            ("K", new[] { "Portugal",     "RD Congo",       "Uzbekistán",      "Colombia"      }),
            ("L", new[] { "Inglaterra",   "Croacia",        "Ghana",           "Panamá"        }),
        };

        (int, int)[] pares = [(0, 1), (2, 3), (0, 2), (1, 3), (0, 3), (1, 2)];

        foreach (var (grupo, equipos) in grupos)
        {
            foreach (var (i, j) in pares)
            {
                partidos.Add(new Partido
                {
                    NumeroPartido = numero++,
                    Fase = Fase.Grupos,
                    FechaHoraUtc = new DateTime(2026, 6, 11, 18, 0, 0, DateTimeKind.Utc),
                    EquipoLocalId = equipoDict[equipos[i]],
                    EquipoVisitanteId = equipoDict[equipos[j]],
                });
            }
        }

        var knockout = new[]
        {
            (73,  Fase.RoundOf32,    "2A",   "2B",    new DateTime(2026, 7,  4, 18, 0, 0, DateTimeKind.Utc)),
            (74,  Fase.RoundOf32,    "1E",   "3ABCDF",new DateTime(2026, 7,  4, 21, 0, 0, DateTimeKind.Utc)),
            (75,  Fase.RoundOf32,    "1F",   "2C",    new DateTime(2026, 7,  5, 18, 0, 0, DateTimeKind.Utc)),
            (76,  Fase.RoundOf32,    "1C",   "2F",    new DateTime(2026, 7,  5, 21, 0, 0, DateTimeKind.Utc)),
            (77,  Fase.RoundOf32,    "1I",   "3CDFGH",new DateTime(2026, 7,  5, 21, 0, 0, DateTimeKind.Utc)),
            (78,  Fase.RoundOf32,    "2E",   "2I",    new DateTime(2026, 7,  6, 18, 0, 0, DateTimeKind.Utc)),
            (79,  Fase.RoundOf32,    "1A",   "3CEFHI",new DateTime(2026, 7,  6, 21, 0, 0, DateTimeKind.Utc)),
            (80,  Fase.RoundOf32,    "1L",   "3EHIJK",new DateTime(2026, 7,  6, 21, 0, 0, DateTimeKind.Utc)),
            (81,  Fase.RoundOf32,    "1D",   "3BEFIJ",new DateTime(2026, 7,  7, 18, 0, 0, DateTimeKind.Utc)),
            (82,  Fase.RoundOf32,    "1G",   "3AEHIJ",new DateTime(2026, 7,  7, 21, 0, 0, DateTimeKind.Utc)),
            (83,  Fase.RoundOf32,    "2K",   "2L",    new DateTime(2026, 7,  7, 21, 0, 0, DateTimeKind.Utc)),
            (84,  Fase.RoundOf32,    "1H",   "2J",    new DateTime(2026, 7,  8, 18, 0, 0, DateTimeKind.Utc)),
            (85,  Fase.RoundOf32,    "1B",   "3EFGIJ",new DateTime(2026, 7,  8, 18, 0, 0, DateTimeKind.Utc)),
            (86,  Fase.RoundOf32,    "1J",   "2H",    new DateTime(2026, 7,  8, 21, 0, 0, DateTimeKind.Utc)),
            (87,  Fase.RoundOf32,    "1K",   "3DEIJL",new DateTime(2026, 7,  8, 21, 0, 0, DateTimeKind.Utc)),
            (88,  Fase.RoundOf32,    "2D",   "2G",    new DateTime(2026, 7,  8, 21, 0, 0, DateTimeKind.Utc)),
            (90,  Fase.RoundOf16,    "G73",  "G75",   new DateTime(2026, 7, 10, 18, 0, 0, DateTimeKind.Utc)),
            (89,  Fase.RoundOf16,    "G74",  "G76",   new DateTime(2026, 7, 10, 21, 0, 0, DateTimeKind.Utc)),
            (91,  Fase.RoundOf16,    "G77",  "G79",   new DateTime(2026, 7, 11, 18, 0, 0, DateTimeKind.Utc)),
            (92,  Fase.RoundOf16,    "G78",  "G80",   new DateTime(2026, 7, 11, 21, 0, 0, DateTimeKind.Utc)),
            (93,  Fase.RoundOf16,    "G81",  "G83",   new DateTime(2026, 7, 12, 18, 0, 0, DateTimeKind.Utc)),
            (94,  Fase.RoundOf16,    "G82",  "G84",   new DateTime(2026, 7, 12, 21, 0, 0, DateTimeKind.Utc)),
            (95,  Fase.RoundOf16,    "G85",  "G87",   new DateTime(2026, 7, 13, 18, 0, 0, DateTimeKind.Utc)),
            (96,  Fase.RoundOf16,    "G86",  "G88",   new DateTime(2026, 7, 13, 21, 0, 0, DateTimeKind.Utc)),
            (97,  Fase.Cuartos,      "G89",  "G90",   new DateTime(2026, 7, 16, 18, 0, 0, DateTimeKind.Utc)),
            (98,  Fase.Cuartos,      "G91",  "G92",   new DateTime(2026, 7, 16, 21, 0, 0, DateTimeKind.Utc)),
            (99,  Fase.Cuartos,      "G93",  "G94",   new DateTime(2026, 7, 17, 18, 0, 0, DateTimeKind.Utc)),
            (100, Fase.Cuartos,      "G95",  "G96",   new DateTime(2026, 7, 17, 21, 0, 0, DateTimeKind.Utc)),
            (101, Fase.Semis,        "G97",  "G98",   new DateTime(2026, 7, 20, 21, 0, 0, DateTimeKind.Utc)),
            (102, Fase.Semis,        "G99",  "G100",  new DateTime(2026, 7, 21, 21, 0, 0, DateTimeKind.Utc)),
            (103, Fase.TercerPuesto, "P101", "P102",  new DateTime(2026, 7, 24, 21, 0, 0, DateTimeKind.Utc)),
            (104, Fase.Final,        "G101", "G102",  new DateTime(2026, 7, 26, 21, 0, 0, DateTimeKind.Utc)),
        };

        foreach (var (num, fase, slotL, slotV, fecha) in knockout)
        {
            partidos.Add(new Partido
            {
                NumeroPartido = num,
                Fase = fase,
                FechaHoraUtc = fecha,
                SlotLocal = slotL,
                SlotVisitante = slotV,
            });
        }

        db.Partidos.AddRange(partidos);
        await db.SaveChangesAsync();
    }
}