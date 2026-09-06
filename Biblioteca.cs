namespace BibliotecaApp;

/// <summary>
/// Núcleo del sistema. Aquí se aplican las tres estructuras de datos exigidas
/// por la guía: DICCIONARIO/MAPA (Dictionary), CONJUNTO (HashSet) y sus combinaciones.
/// </summary>
public class Biblioteca
{
    // 1) MAPA (Dictionary<TKey, TValue>): catálogo principal indexado por ISBN.
    //    Permite registrar y consultar un libro en tiempo O(1) en promedio,
    //    sin recorrer toda la colección.
    private readonly Dictionary<string, Libro> _catalogoPorIsbn = new();

    // 2) CONJUNTO (HashSet<T>): almacena los géneros literarios sin repetidos.
    //    Un HashSet garantiza que cada género exista una sola vez,
    //    sin importar cuántas veces se registre un libro de ese género.
    private readonly HashSet<string> _generos = new();

    // 3) MAPA de CONJUNTOS (Dictionary<string, HashSet<string>>):
    //    para cada género, un conjunto de ISBN (evita ISBN duplicados por género
    //    y permite listar rápidamente todos los libros de una categoría).
    private readonly Dictionary<string, HashSet<string>> _isbnPorGenero = new();

    // 4) MAPA (Dictionary<string, List<string>>): agrupa los ISBN por autor.
    //    Se usa List porque un mismo autor puede tener varios libros y
    //    aquí sí interesa el orden de registro, no la unicidad.
    private readonly Dictionary<string, List<string>> _isbnPorAutor = new();

    /// <summary>
    /// Registra un nuevo libro. Retorna false si el ISBN ya existe (evita duplicados).
    /// </summary>
    public bool RegistrarLibro(Libro libro)
    {
        if (_catalogoPorIsbn.ContainsKey(libro.Isbn))
        {
            return false;
        }

        _catalogoPorIsbn[libro.Isbn] = libro;

        _generos.Add(libro.Genero);

        if (!_isbnPorGenero.TryGetValue(libro.Genero, out var isbnsDelGenero))
        {
            isbnsDelGenero = new HashSet<string>();
            _isbnPorGenero[libro.Genero] = isbnsDelGenero;
        }
        isbnsDelGenero.Add(libro.Isbn);

        if (!_isbnPorAutor.TryGetValue(libro.Autor, out var isbnsDelAutor))
        {
            isbnsDelAutor = new List<string>();
            _isbnPorAutor[libro.Autor] = isbnsDelAutor;
        }
        isbnsDelAutor.Add(libro.Isbn);

        return true;
    }

    /// <summary>
    /// Búsqueda directa por clave en el diccionario: O(1) en promedio.
    /// </summary>
    public Libro? BuscarPorIsbn(string isbn)
    {
        _catalogoPorIsbn.TryGetValue(isbn, out var libro);
        return libro;
    }

    /// <summary>
    /// Elimina un libro del catálogo y de todas las estructuras auxiliares.
    /// </summary>
    public bool EliminarLibro(string isbn)
    {
        if (!_catalogoPorIsbn.TryGetValue(isbn, out var libro))
        {
            return false;
        }

        _catalogoPorIsbn.Remove(isbn);
        _isbnPorGenero[libro.Genero].Remove(isbn);
        _isbnPorAutor[libro.Autor].Remove(isbn);

        return true;
    }

    /// <summary>
    /// Devuelve el conjunto de géneros únicos registrados hasta el momento.
    /// </summary>
    public IReadOnlySet<string> ObtenerGeneros() => _generos;

    /// <summary>
    /// Lista todos los libros de un género dado, usando el mapa de conjuntos.
    /// </summary>
    public List<Libro> ListarPorGenero(string genero)
    {
        var resultado = new List<Libro>();
        if (_isbnPorGenero.TryGetValue(genero, out var isbns))
        {
            foreach (var isbn in isbns)
            {
                resultado.Add(_catalogoPorIsbn[isbn]);
            }
        }
        return resultado;
    }

    /// <summary>
    /// Lista todos los libros de un autor, usando el mapa autor -> lista de ISBN.
    /// </summary>
    public List<Libro> ListarPorAutor(string autor)
    {
        var resultado = new List<Libro>();
        if (_isbnPorAutor.TryGetValue(autor, out var isbns))
        {
            foreach (var isbn in isbns)
            {
                resultado.Add(_catalogoPorIsbn[isbn]);
            }
        }
        return resultado;
    }

    public int TotalLibros => _catalogoPorIsbn.Count;

    public IEnumerable<Libro> TodosLosLibros() => _catalogoPorIsbn.Values;

    public IEnumerable<string> TodosLosAutores() => _isbnPorAutor.Keys;

    /// <summary>
    /// Reportería: cuenta cuántos libros hay por cada género.
    /// </summary>
    public Dictionary<string, int> ReporteConteoPorGenero()
    {
        var reporte = new Dictionary<string, int>();
        foreach (var genero in _generos)
        {
            reporte[genero] = _isbnPorGenero[genero].Count;
        }
        return reporte;
    }
}
