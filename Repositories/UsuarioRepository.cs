using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using pyreApi.Data;
using pyreApi.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Text;

namespace pyreApi.Repositories
{
    public class UsuarioRepository : GenericRepository<Usuario>
    {
        private readonly ILogger<UsuarioRepository> _logger;
        private readonly IConfiguration _configuration;

        public UsuarioRepository(ApplicationDbContext context, ILogger<UsuarioRepository> logger, IConfiguration configuration) : base(context)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<Usuario?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<Usuario?> GetByLegajoAsync(string legajo)
        {
            // Validar longitud del legajo
            if (!string.IsNullOrEmpty(legajo) && legajo.Length > 5)
            {
                _logger.LogWarning("Intento de búsqueda con legajo que excede 5 caracteres: {Legajo}", legajo);
                return null;
            }

            return await _dbSet.FirstOrDefaultAsync(u => u.Legajo == legajo);
        }

        public async Task<Usuario?> GetByDniAsync(string dni)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Dni == dni);
        }

        public async Task<IEnumerable<Usuario>> GetByRolAsync(int rolId)
        {
            return await _dbSet.Where(u => u.RolId == rolId).ToListAsync();
        }

        public async Task<IEnumerable<Usuario>> GetAllWithRolAsync()
        {
            return await _dbSet.Include(u => u.Rol).ToListAsync();
        }

        public async Task<Usuario?> GetByIdWithRolAsync(int id)
        {
            return await _dbSet.Include(u => u.Rol).FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<bool> ValidateCredentialsAsync(string legajo, string password)
        {
            try
            {
                _logger.LogInformation("Iniciando validación de credenciales para legajo: {Legajo}", legajo);

                if (string.IsNullOrWhiteSpace(legajo) || string.IsNullOrWhiteSpace(password))
                {
                    _logger.LogWarning("Intento de validación con legajo o contraseña vacíos");
                    return false;
                }

                // Validar longitud del legajo
                if (legajo.Length > 5)
                {
                    _logger.LogWarning("Intento de validación con legajo que excede 5 caracteres: {Legajo}", legajo);
                    return false;
                }

                var usuario = await _context.Usuario
                    .FirstOrDefaultAsync(u => u.Legajo == legajo);

                if (usuario == null)
                {
                    _logger.LogWarning("No se encontró usuario con legajo: {Legajo}", legajo);
                    return false;
                }

                _logger.LogInformation("Usuario encontrado para validación. Tiene contraseña configurada: {HasPassword}",
                    !string.IsNullOrEmpty(usuario.PasswordHash));

                // Check if user has a password set
                if (string.IsNullOrEmpty(usuario.PasswordHash))
                {
                    _logger.LogWarning("El usuario con legajo {Legajo} no tiene contraseña configurada", legajo);
                    return false;
                }

                // Verify password using KeyDerivation (matching the hash method)
                bool isValid = VerifyPasswordWithKeyDerivation(password, usuario.PasswordHash);

                _logger.LogInformation("Validación de credenciales completada para legajo {Legajo}: {IsValid}", legajo, isValid);

                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico al validar credenciales para legajo: {Legajo}", legajo);
                return false;
            }
        }

        public async Task<IEnumerable<Usuario>> GetActiveUsersAsync()
        {
            return await _dbSet.Where(u => u.Activo == true).ToListAsync();
        }

        public async Task<IEnumerable<Usuario>> GetUsersWithSystemAccessAsync()
        {
            return await _dbSet.Where(u => u.AccedeAlSistema == true && u.Activo == true).ToListAsync();
        }

        public async Task<Usuario?> GetByLegajoWithRolAsync(string legajo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(legajo))
                {
                    _logger.LogWarning("Intento de búsqueda con legajo vacío o nulo");
                    return null;
                }

                // Validar longitud del legajo
                if (legajo.Length > 5)
                {
                    _logger.LogWarning("Intento de búsqueda con legajo que excede 5 caracteres: {Legajo}", legajo);
                    return null;
                }

                _logger.LogInformation("Buscando usuario por legajo: {Legajo}", legajo);

                var usuario = await _context.Usuario
                    .Include(u => u.Rol)
                    .FirstOrDefaultAsync(u => u.Legajo == legajo);

                _logger.LogInformation("Resultado de búsqueda por legajo {Legajo}: {Found}",
                    legajo, usuario != null ? "Usuario encontrado" : "Usuario no encontrado");

                return usuario;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico al buscar usuario por legajo: {Legajo}", legajo);
                throw;
            }
        }

        // Método auxiliar para verificar contraseña usando KeyDerivation
        private bool VerifyPasswordWithKeyDerivation(string password, string hashedPassword)
        {
            try
            {
                // Salt fijo (debe coincidir con el usado para hashear)
                string salt = _configuration["Salt"]; // Lee el salt del archivo de configuración
                if (string.IsNullOrEmpty(salt))
                {
                    _logger.LogError("El valor de 'Salt' no está configurado en appsettings.json");
                    throw new InvalidOperationException("Configuración de seguridad incompleta. Contacte al administrador del sistema.");
                }

                // Hashear la contraseña ingresada con el mismo método
                string computedHash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: password,
                    salt: Encoding.ASCII.GetBytes(salt),
                    prf: KeyDerivationPrf.HMACSHA1,
                    iterationCount: 10000,
                    numBytesRequested: 256 / 8));

                // Comparar los hashes
                return computedHash == hashedPassword;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar contraseña con KeyDerivation");
                return false;
            }
        }
    }
}