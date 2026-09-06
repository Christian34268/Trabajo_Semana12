using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ColaAtraccionParque
{
    // ============================================================
    // CLASE: Persona
    // Representa a cada cliente que llega a la fila de la atracción
    // ============================================================
    public class Persona
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public DateTime HoraLlegada { get; set; }

        public Persona(int id, string nombre)
        {
            Id = id;
            Nombre = nombre;
            HoraLlegada = DateTime.Now;
        }

        public override string ToString()
        {
            return $"[#{Id}] {Nombre} - Llegó: {HoraLlegada:HH:mm:ss}";
        }
    }

    // ============================================================
    // CLASE: Asiento
    // Representa cada uno de los 30 asientos de la atracción
    // ============================================================
    public class Asiento
    {
        public int Numero { get; set; }
        public bool Ocupado { get; set; }
        public Persona PersonaAsignada { get; set; }

        public Asiento(int numero)
        {
            Numero = numero;
            Ocupado = false;
            PersonaAsignada = null;
        }

        public override string ToString()
        {
            string estado = Ocupado ? $"OCUPADO por {PersonaAsignada.Nombre}" : "LIBRE";
            return $"Asiento {Numero:D2}: {estado}";
        }
    }

    // ============================================================
    // NODO genérico para la cola implementada manualmente
    // ============================================================
    public class Nodo<T>
    {
        public T Valor { get; set; }
        public Nodo<T> Siguiente { get; set; }

        public Nodo(T valor)
        {
            Valor = valor;
            Siguiente = null;
        }
    }

    // ============================================================
    // CLASE: ColaPersonalizada<T>
    // Implementación PROPIA de una cola (FIFO) usando nodos enlazados.
    // No se utiliza Queue<T> de .NET: se construye la estructura desde cero
    // para evidenciar el aprendizaje del Tipo Abstracto de Datos "Cola".
    // ============================================================
    public class ColaPersonalizada<T>
    {
        private Nodo<T> frente;   // Primer elemento (el próximo en salir)
        private Nodo<T> final;    // Último elemento (el último que entró)
        private int contador;

        public int Count => contador;

        public ColaPersonalizada()
        {
            frente = null;
            final = null;
            contador = 0;
        }

        // Encolar: agrega un elemento al FINAL de la cola -> O(1)
        public void Encolar(T elemento)
        {
            Nodo<T> nuevoNodo = new Nodo<T>(elemento);

            if (EstaVacia())
            {
                frente = nuevoNodo;
                final = nuevoNodo;
            }
            else
            {
                final.Siguiente = nuevoNodo;
                final = nuevoNodo;
            }
            contador++;
        }

        // Desencolar: saca y retorna el elemento del FRENTE de la cola -> O(1)
        public T Desencolar()
        {
            if (EstaVacia())
                throw new InvalidOperationException("La cola está vacía. No hay personas esperando.");

            T valor = frente.Valor;
            frente = frente.Siguiente;
            contador--;

            if (frente == null)
                final = null;

            return valor;
        }

        // Ver el primer elemento sin sacarlo (Peek) -> O(1)
        public T VerFrente()
        {
            if (EstaVacia())
                throw new InvalidOperationException("La cola está vacía.");
            return frente.Valor;
        }

        public bool EstaVacia()
        {
            return frente == null;
        }

        // Recorre la cola para reportería (visualizar todos los elementos) -> O(n)
        public List<T> ObtenerTodos()
        {
            List<T> lista = new List<T>();
            Nodo<T> actual = frente;
            while (actual != null)
            {
                lista.Add(actual.Valor);
                actual = actual.Siguiente;
            }
            return lista;
        }
    }

    // ============================================================
    // CLASE: GestorAtraccion
    // Contiene la lógica de negocio: administra la cola de espera
    // y los 30 asientos, asignándolos en orden de llegada (FIFO).
    // ============================================================
    public class GestorAtraccion
    {
        private const int TOTAL_ASIENTOS = 30;

        private ColaPersonalizada<Persona> filaDeEspera;
        private List<Asiento> asientos;
        private List<Persona> historialAsignados;
        private int siguienteId;

        public GestorAtraccion()
        {
            filaDeEspera = new ColaPersonalizada<Persona>();
            asientos = new List<Asiento>();
            historialAsignados = new List<Persona>();
            siguienteId = 1;

            for (int i = 1; i <= TOTAL_ASIENTOS; i++)
                asientos.Add(new Asiento(i));
        }

        // Agrega una persona a la fila de espera (Encolar)
        public void AgregarPersonaAFila(string nombre)
        {
            Persona nueva = new Persona(siguienteId++, nombre);
            filaDeEspera.Encolar(nueva);
            Console.WriteLine($"-> {nueva.Nombre} se unió a la fila (posición {filaDeEspera.Count}).");
        }

        // Asigna el siguiente asiento libre a la primera persona de la fila
        public bool AsignarSiguienteAsiento()
        {
            if (filaDeEspera.EstaVacia())
            {
                Console.WriteLine("No hay personas en la fila esperando.");
                return false;
            }

            Asiento asientoLibre = asientos.Find(a => !a.Ocupado);
            if (asientoLibre == null)
            {
                Console.WriteLine("Todos los asientos están vendidos/ocupados (30/30).");
                return false;
            }

            Persona persona = filaDeEspera.Desencolar();
            asientoLibre.Ocupado = true;
            asientoLibre.PersonaAsignada = persona;
            historialAsignados.Add(persona);

            Console.WriteLine($"Asiento {asientoLibre.Numero:D2} asignado a {persona.Nombre} (orden de llegada respetado).");
            return true;
        }

        // Procesa TODA la fila de una sola vez (hasta llenar los 30 asientos o vaciar la fila)
        public void ProcesarTodaLaFila()
        {
            Stopwatch cronometro = Stopwatch.StartNew();
            int asignados = 0;

            while (!filaDeEspera.EstaVacia())
            {
                bool exito = AsignarSiguienteAsiento();
                if (!exito) break;
                asignados++;
            }

            cronometro.Stop();
            Console.WriteLine($"\n--- Se asignaron {asignados} asiento(s) ---");
            Console.WriteLine($"--- Tiempo de ejecución: {cronometro.Elapsed.TotalMilliseconds} ms " +
                               $"({cronometro.ElapsedTicks} ticks) ---\n");
        }

        // ---------------- REPORTERÍA ----------------

        public void MostrarFilaDeEspera()
        {
            Console.WriteLine("\n===== FILA DE ESPERA (orden FIFO) =====");
            var personasEnFila = filaDeEspera.ObtenerTodos();

            if (personasEnFila.Count == 0)
            {
                Console.WriteLine("(No hay nadie esperando en este momento)");
            }
            else
            {
                int posicion = 1;
                foreach (var p in personasEnFila)
                {
                    Console.WriteLine($"{posicion}. {p}");
                    posicion++;
                }
            }
            Console.WriteLine("========================================\n");
        }

        public void MostrarMapaDeAsientos()
        {
            Console.WriteLine("\n===== MAPA DE LOS 30 ASIENTOS =====");
            foreach (var asiento in asientos)
                Console.WriteLine(asiento);

            int ocupados = asientos.FindAll(a => a.Ocupado).Count;
            Console.WriteLine($"------------------------------------");
            Console.WriteLine($"Ocupados: {ocupados}/{TOTAL_ASIENTOS}  |  Libres: {TOTAL_ASIENTOS - ocupados}");
            Console.WriteLine("=====================================\n");
        }

        public void ConsultarPersona(string nombre)
        {
            var enAsiento = historialAsignados.Find(p => p.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase));
            if (enAsiento != null)
            {
                var asiento = asientos.Find(a => a.PersonaAsignada == enAsiento);
                Console.WriteLine($"{enAsiento.Nombre} ya tiene asiento asignado: Asiento {asiento.Numero:D2}.");
                return;
            }

            var enFila = filaDeEspera.ObtenerTodos();
            int idx = enFila.FindIndex(p => p.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase));
            if (idx >= 0)
            {
                Console.WriteLine($"{nombre} está en la fila de espera, posición {idx + 1} de {enFila.Count}.");
            }
            else
            {
                Console.WriteLine($"No se encontró a '{nombre}' ni en la fila ni con asiento asignado.");
            }
        }

        public bool QuedanAsientosLibres()
        {
            return asientos.Exists(a => !a.Ocupado);
        }

        public int PersonasEnFila => filaDeEspera.Count;
    }

    // ============================================================
    // PROGRAMA PRINCIPAL: Menú por consola
    // ============================================================
    class Program
    {
        static void Main(string[] args)
        {
            GestorAtraccion gestor = new GestorAtraccion();
            bool salir = false;

            Console.WriteLine("=================================================");
            Console.WriteLine(" SIMULADOR DE FILA - ATRACCIÓN DE PARQUE (30 ASIENTOS)");
            Console.WriteLine(" Estructura de datos utilizada: COLA (FIFO)");
            Console.WriteLine("=================================================\n");

            while (!salir)
            {
                MostrarMenu();
                string opcion = Console.ReadLine();
                Console.WriteLine();

                switch (opcion)
                {
                    case "1":
                        Console.Write("Nombre de la persona: ");
                        string nombre = Console.ReadLine();
                        gestor.AgregarPersonaAFila(nombre);
                        break;

                    case "2":
                        gestor.AsignarSiguienteAsiento();
                        break;

                    case "3":
                        gestor.ProcesarTodaLaFila();
                        break;

                    case "4":
                        gestor.MostrarFilaDeEspera();
                        break;

                    case "5":
                        gestor.MostrarMapaDeAsientos();
                        break;

                    case "6":
                        Console.Write("Nombre a consultar: ");
                        string buscar = Console.ReadLine();
                        gestor.ConsultarPersona(buscar);
                        break;

                    case "7":
                        CargarDatosDePrueba(gestor);
                        break;

                    case "0":
                        salir = true;
                        Console.WriteLine("¡Hasta luego!");
                        break;

                    default:
                        Console.WriteLine("Opción no válida.");
                        break;
                }

                Console.WriteLine();
            }
        }

        static void MostrarMenu()
        {
            Console.WriteLine("------------------- MENÚ -------------------");
            Console.WriteLine("1. Agregar persona a la fila (Encolar)");
            Console.WriteLine("2. Asignar el siguiente asiento (Desencolar 1)");
            Console.WriteLine("3. Procesar TODA la fila (Desencolar todo)");
            Console.WriteLine("4. Reporte: ver fila de espera actual");
            Console.WriteLine("5. Reporte: ver mapa de los 30 asientos");
            Console.WriteLine("6. Consultar una persona por nombre");
            Console.WriteLine("7. Cargar datos de prueba (32 personas)");
            Console.WriteLine("0. Salir");
            Console.Write("Elige una opción: ");
        }

        // Carga datos de ejemplo para probar rápido (incluye 2 personas de más
        // para demostrar el caso de "todos los asientos vendidos")
        static void CargarDatosDePrueba(GestorAtraccion gestor)
        {
            string[] nombres = {
                "Ana", "Luis", "María", "Carlos", "Sofía", "Pedro", "Laura", "Diego",
                "Valentina", "Andrés", "Camila", "José", "Daniela", "Miguel", "Paula",
                "Fernando", "Gabriela", "Ricardo", "Isabella", "Javier", "Natalia",
                "Sebastián", "Renata", "Iván", "Mariana", "Óscar", "Lucía", "Emilio",
                "Fabiola", "Rodrigo", "Ximena", "Tomás"
            };

            foreach (var n in nombres)
                gestor.AgregarPersonaAFila(n);

            Console.WriteLine($"\nSe cargaron {nombres.Length} personas de prueba " +
                               $"(32 personas para 30 asientos: 2 quedarán en espera).");
        }
    }
}
