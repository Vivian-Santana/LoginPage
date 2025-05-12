namespace LoginPage.Modelo
{
    public class UsuarioModelo
    {
        public UsuarioModelo(string name)
        {
            Name = name;
            Id = Guid.NewGuid(); // *
        }

        //Guid gera ids únicos com uma gama maior de caracteres dificultando o risco de ter ids iguias
        public Guid Id { get; init; } //*id é inicializado no construtor

        public string Name { get; private set; } = String.Empty;

    }
    
}
