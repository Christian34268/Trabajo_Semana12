namespace BibliotecaApp;

/// <summary>
/// Representa un libro dentro del sistema de la biblioteca.
/// </summary>
public class Libro
{
    public string Isbn { get; set; }
    public string Titulo { get; set; }
    public string Autor { get; set; }
    public string Genero { get; set; }
    public int AnioPublicacion { get; set; }
    public bool Disponible { get; set; }

    public Libro(string isbn, string titulo, string autor, string genero, int anioPublicacion)
    {
        Isbn = isbn;
        Titulo = titulo;
        Autor = autor;
        Genero = genero;
        AnioPublicacion = anioPublicacion;
        Disponible = true;
    }

    public override string ToString()
    {
        string estado = Disponible ? "Disponible" : "Prestado";
        return $"ISBN: {Isbn} | \"{Titulo}\" | Autor: {Autor} | Género: {Genero} | Año: {AnioPublicacion} | {estado}";
    }
}
