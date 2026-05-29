using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuinielaVinccler.UI.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class ActualizarFechasPartidos : Migration
    {
        // Fuente: calendario oficial FIFA 2026 (Al Jazeera / FIFA.com)
        // Todos los horarios convertidos a UTC.
        // Venezuela = UTC-4 (para referencia: hora VEN = UTC - 4h)
        //
        // Mapeo verificado partido a partido contra el seed de la DB.
        //
        // Jornada 1: primer enfrentamiento de cada par de equipos por grupo
        // Jornada 2: segundo enfrentamiento
        // Jornada 3: tercer enfrentamiento (simultáneos por grupo)

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var updates = new (int Numero, string FechaUtc)[]
            {
                // ── GRUPO A ───────────────────────────────────────────────────
                // J1: Jue 11 Jun
                (1,  "2026-06-11 19:00:00"), // México vs Sudáfrica          15:00 VEN
                (2,  "2026-06-12 02:00:00"), // Corea del Sur vs Rep. Checa  22:00 VEN J11
                // J2: Jue 18 Jun
                (3,  "2026-06-19 03:00:00"), // México vs Corea del Sur      23:00 VEN J18
                (4,  "2026-06-18 17:00:00"), // Sudáfrica vs Rep. Checa      13:00 VEN
                // J3: Mié 24 Jun (simultáneos)
                (5,  "2026-06-25 03:00:00"), // México vs Rep. Checa         23:00 VEN J24
                (6,  "2026-06-25 03:00:00"), // Sudáfrica vs Corea del Sur   23:00 VEN J24

                // ── GRUPO B ───────────────────────────────────────────────────
                // J1: Vie 12 Jun
                (7,  "2026-06-12 19:00:00"), // Canadá vs Bosnia             15:00 VEN
                (8,  "2026-06-13 19:00:00"), // Catar vs Suiza               15:00 VEN
                // J2: Jue 18 Jun
                (9,  "2026-06-19 02:00:00"), // Canadá vs Catar              22:00 VEN J18
                (10, "2026-06-18 23:00:00"), // Bosnia vs Suiza              19:00 VEN
                // J3: Mié 24 Jun (simultáneos)
                (11, "2026-06-24 23:00:00"), // Canadá vs Suiza              19:00 VEN
                (12, "2026-06-24 23:00:00"), // Bosnia vs Catar              19:00 VEN

                // ── GRUPO C ───────────────────────────────────────────────────
                // J1: Sáb 13 Jun
                (13, "2026-06-13 23:00:00"), // Brasil vs Marruecos          19:00 VEN
                (14, "2026-06-14 01:00:00"), // Haití vs Escocia             21:00 VEN J13
                // J2: Vie 19 Jun
                (15, "2026-06-20 02:00:00"), // Brasil vs Haití              22:00 VEN J19
                (16, "2026-06-19 23:00:00"), // Marruecos vs Escocia         19:00 VEN
                // J3: Mié 24 Jun (simultáneos)
                (17, "2026-06-24 23:00:00"), // Brasil vs Escocia            19:00 VEN
                (18, "2026-06-24 23:00:00"), // Marruecos vs Haití           19:00 VEN

                // ── GRUPO D ───────────────────────────────────────────────────
                // J1: Vie 12 Jun
                (19, "2026-06-13 01:00:00"), // EEUU vs Paraguay             21:00 VEN J12
                (20, "2026-06-14 04:00:00"), // Australia vs Turquía         00:00 VEN J14
                // J2: Vie 19 Jun
                (21, "2026-06-19 23:00:00"), // EEUU vs Australia            19:00 VEN
                (22, "2026-06-20 08:00:00"), // Paraguay vs Turquía          04:00 VEN J20
                // J3: Jue 25 Jun (simultáneos)
                (23, "2026-06-26 06:00:00"), // EEUU vs Turquía              02:00 VEN J26
                (24, "2026-06-26 06:00:00"), // Paraguay vs Australia        02:00 VEN J26

                // ── GRUPO E ───────────────────────────────────────────────────
                // J1: Dom 14 Jun
                (25, "2026-06-14 18:00:00"), // Alemania vs Curazao          14:00 VEN
                (26, "2026-06-15 00:00:00"), // Costa de Marfil vs Ecuador   20:00 VEN J14
                // J2: Sáb 20 Jun
                (27, "2026-06-20 21:00:00"), // Alemania vs Costa de Marfil  17:00 VEN
                (28, "2026-06-21 04:00:00"), // Curazao vs Ecuador           00:00 VEN J21
                // J3: Jue 25 Jun (simultáneos)
                (29, "2026-06-25 21:00:00"), // Alemania vs Ecuador          17:00 VEN
                (30, "2026-06-25 21:00:00"), // Curazao vs Costa de Marfil   17:00 VEN

                // ── GRUPO F ───────────────────────────────────────────────────
                // J1: Dom 14 Jun
                (31, "2026-06-14 21:00:00"), // Países Bajos vs Japón        17:00 VEN
                (32, "2026-06-15 04:00:00"), // Suecia vs Túnez              00:00 VEN J15
                // J2: Sáb 20 Jun
                (33, "2026-06-20 19:00:00"), // Países Bajos vs Suecia       15:00 VEN
                (34, "2026-06-21 06:00:00"), // Japón vs Túnez               02:00 VEN J21
                // J3: Jue 25 Jun (simultáneos)
                (35, "2026-06-26 01:00:00"), // Países Bajos vs Túnez        21:00 VEN J25
                (36, "2026-06-26 01:00:00"), // Japón vs Suecia              21:00 VEN J25

                // ── GRUPO G ───────────────────────────────────────────────────
                // J1: Lun 15 Jun
                (37, "2026-06-15 23:00:00"), // Bélgica vs Egipto            19:00 VEN
                (38, "2026-06-16 05:00:00"), // Irán vs Nueva Zelanda        01:00 VEN J16
                // J2: Dom 21 Jun
                (39, "2026-06-21 23:00:00"), // Bélgica vs Irán              19:00 VEN
                (40, "2026-06-22 05:00:00"), // Egipto vs Nueva Zelanda      01:00 VEN J22
                // J3: Vie 26 Jun (simultáneos)
                (41, "2026-06-27 07:00:00"), // Bélgica vs Nueva Zelanda     03:00 VEN J27
                (42, "2026-06-27 07:00:00"), // Egipto vs Irán               03:00 VEN J27

                // ── GRUPO H ───────────────────────────────────────────────────
                // J1: Lun 15 Jun
                (43, "2026-06-15 17:00:00"), // España vs Cabo Verde         13:00 VEN
                (44, "2026-06-15 23:00:00"), // Arabia Saudita vs Uruguay    19:00 VEN
                // J2: Dom 21 Jun
                (45, "2026-06-21 17:00:00"), // España vs Arabia Saudita     13:00 VEN
                (46, "2026-06-21 23:00:00"), // Cabo Verde vs Uruguay        19:00 VEN
                // J3: Vie 26 Jun (simultáneos)
                (47, "2026-06-27 02:00:00"), // España vs Uruguay            22:00 VEN J26
                (48, "2026-06-27 02:00:00"), // Cabo Verde vs Arabia Saudita 22:00 VEN J26

                // ── GRUPO I ───────────────────────────────────────────────────
                // J1: Mar 16 Jun
                (49, "2026-06-16 20:00:00"), // Francia vs Senegal           16:00 VEN
                (50, "2026-06-16 23:00:00"), // Irak vs Noruega              19:00 VEN
                // J2: Lun 22 Jun
                (51, "2026-06-22 22:00:00"), // Francia vs Irak              18:00 VEN
                (52, "2026-06-23 01:00:00"), // Senegal vs Noruega           21:00 VEN J22
                // J3: Vie 26 Jun (simultáneos)
                (53, "2026-06-26 20:00:00"), // Francia vs Noruega           16:00 VEN
                (54, "2026-06-26 20:00:00"), // Senegal vs Irak              16:00 VEN

                // ── GRUPO J ───────────────────────────────────────────────────
                // J1: Mar 16 Jun
                (55, "2026-06-17 03:00:00"), // Argentina vs Argelia         23:00 VEN J16
                (56, "2026-06-17 08:00:00"), // Austria vs Jordania          04:00 VEN J17
                // J2: Lun 22 Jun
                (57, "2026-06-22 19:00:00"), // Argentina vs Austria         15:00 VEN
                (58, "2026-06-23 07:00:00"), // Argelia vs Jordania          03:00 VEN J23
                // J3: Sáb 27 Jun (simultáneos)
                (59, "2026-06-28 04:00:00"), // Argentina vs Jordania        00:00 VEN J28
                (60, "2026-06-28 04:00:00"), // Argelia vs Austria           00:00 VEN J28

                // ── GRUPO K ───────────────────────────────────────────────────
                // J1: Mié 17 Jun
                (61, "2026-06-17 19:00:00"), // Portugal vs RD Congo         15:00 VEN
                (62, "2026-06-18 04:00:00"), // Uzbekistán vs Colombia       00:00 VEN J18
                // J2: Mar 23 Jun
                (63, "2026-06-23 19:00:00"), // Portugal vs Uzbekistán       15:00 VEN
                (64, "2026-06-24 04:00:00"), // RD Congo vs Colombia         00:00 VEN J24
                // J3: Sáb 27 Jun (simultáneos)
                (65, "2026-06-28 02:30:00"), // Portugal vs Colombia         22:30 VEN J27
                (66, "2026-06-28 02:30:00"), // RD Congo vs Uzbekistán       22:30 VEN J27

                // ── GRUPO L ───────────────────────────────────────────────────
                // J1: Mié 17 Jun
                (67, "2026-06-17 22:00:00"), // Inglaterra vs Croacia        18:00 VEN
                (68, "2026-06-18 00:00:00"), // Ghana vs Panamá              20:00 VEN J17
                // J2: Mar 23 Jun
                (69, "2026-06-23 21:00:00"), // Inglaterra vs Ghana          17:00 VEN
                (70, "2026-06-24 00:00:00"), // Croacia vs Panamá            20:00 VEN J23
                // J3: Sáb 27 Jun (simultáneos)
                (71, "2026-06-27 22:00:00"), // Inglaterra vs Panamá         18:00 VEN
                (72, "2026-06-27 22:00:00"), // Croacia vs Ghana             18:00 VEN
            };

            foreach (var (numero, fecha) in updates)
            {
                migrationBuilder.Sql(
                    $"UPDATE \"Partidos\" SET \"FechaHoraUtc\" = '{fecha}' WHERE \"NumeroPartido\" = {numero};");
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE \"Partidos\" SET \"FechaHoraUtc\" = '2026-06-11 18:00:00' WHERE \"NumeroPartido\" <= 72;");
        }
    }
}