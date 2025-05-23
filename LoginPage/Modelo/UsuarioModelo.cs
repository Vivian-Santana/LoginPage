namespace LoginPage.Modelo
{
    public class UsuarioModelo
    {
        public UsuarioModelo(string name, string SenhaHash)
        {
            Name = name;
            Id = Guid.NewGuid(); // *
            this.SenhaHash = SenhaHash;
        }
        public UsuarioModelo() 
        { }

        public bool Status { get; set; } = true;

        //Guid gera ids únicos com uma gama maior de caracteres dificultando o risco de ter ids iguias
        public Guid Id { get; init; } //*id é inicializado no construtor

        public string Name { get; private set; } = String.Empty;

        //assegura contra mudanças acidentais em Nome que esta privado para setar
        public void MudarNome(string name)
        {
            Name = name;
        }

        //Flag - para desativar ao invés de deletar o usuario completamente (soft delete)
        
        public void SetInativo()
        {
            Status = false;
        }

        public string SenhaHash { get; set; } = string.Empty;

        public void MudarSenha(string novaSenhaHash)
        {
            SenhaHash = novaSenhaHash;
        }
    }

}
