using System;

namespace PowerFit
{
    /// <summary>
    /// Representa um plano de assinatura da academia (ex.: Mensal, Trimestral, Anual).
    /// Um Plano existe de forma independente e pode ser vinculado a várias Matrículas.
    /// </summary>
    public class Plano
    {
        public string Nome { get; private set; }
        public decimal ValorMensal { get; private set; }

        public Plano(string nome, decimal valorMensal)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("O nome do plano não pode ser vazio.");

            if (valorMensal <= 0)
                throw new ArgumentException("O valor mensal do plano deve ser maior que zero.");

            Nome = nome;
            ValorMensal = valorMensal;
        }
    }

    /// <summary>
    /// Representa um aluno cadastrado na academia. Um Aluno existe de forma independente
    /// de qualquer Plano/Matrícula (pode estar cadastrado sem matrícula ativa no momento).
    /// </summary>
    public class Aluno
    {
        public string Nome { get; private set; }
        public string Cpf { get; private set; }

        private double _peso;
        public double Peso
        {
            get { return _peso; }
            private set
            {
                if (value <= 0)
                    throw new ArgumentException("O peso não pode ser zerado ou negativo no sistema.");

                _peso = value;
            }
        }

        // Composição: a Matrícula é instanciada e tem seu ciclo de vida controlado pelo Aluno.
        // Ninguém de fora cria uma Matrícula "solta"; ela nasce a partir de Aluno.Matricular().
        // É anulável (Matricula?) porque um Aluno pode estar cadastrado sem nenhuma matrícula ativa.
        public Matricula? MatriculaAtiva { get; private set; }

        public Aluno(string nome, string cpf, double pesoInicial)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("O nome do aluno não pode ser vazio.");

            if (string.IsNullOrWhiteSpace(cpf))
                throw new ArgumentException("O CPF do aluno não pode ser vazio.");

            Nome = nome;
            Cpf = cpf;
            Peso = pesoInicial;
        }

        public void AtualizarPeso(double novoPeso)
        {
            Peso = novoPeso;
            Console.WriteLine($"O peso de {Nome} foi atualizado para {Peso}kg.");
        }

        public void Matricular(Plano plano)
        {
            if (plano == null)
                throw new ArgumentNullException(nameof(plano), "Não é possível matricular sem um plano.");

            MatriculaAtiva = new Matricula(this, plano);
            Console.WriteLine($"{Nome} foi matriculado no plano {plano.Nome}.");
        }
    }

    /// <summary>
    /// Representa o registro da decisão de um Aluno por um Plano, com data de início.
    /// Só faz sentido existir vinculada a um Aluno e a um Plano já existentes.
    /// </summary>
    public class Matricula
    {
        // Agregação: a Matrícula referencia um Plano que existe de forma independente
        // dela e pode ser compartilhado por outras Matrículas (não é "dono" do Plano).
        public Plano PlanoVinculado { get; private set; }

        // Associação: ligação com o Aluno que originou a matrícula.
        public Aluno AlunoVinculado { get; private set; }

        public DateTime DataInicio { get; private set; }

        public Matricula(Aluno aluno, Plano plano)
        {
            if (aluno == null)
                throw new ArgumentNullException(nameof(aluno), "Uma matrícula precisa de um aluno existente.");

            if (plano == null)
                throw new ArgumentNullException(nameof(plano), "Uma matrícula precisa de um plano existente.");

            AlunoVinculado = aluno;
            PlanoVinculado = plano;
            DataInicio = DateTime.Now;
        }
    }

    /// <summary>
    /// Representa um Personal Trainer, que orienta Alunos em treinos pontuais,
    /// sem vínculo permanente com nenhum Aluno específico.
    /// </summary>
    public class PersonalTrainer
    {
        public string Nome { get; private set; }

        public PersonalTrainer(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("O nome do personal trainer não pode ser vazio.");

            Nome = nome;
        }

        // Associação: o Personal Trainer interage com o Aluno pontualmente, sem vínculo estrutural.
        public void OrientarTreino(Aluno aluno)
        {
            if (aluno == null)
                throw new ArgumentNullException(nameof(aluno), "Não é possível orientar um treino sem um aluno.");

            Console.WriteLine($"O Personal Trainer {Nome} está orientando o treino de {aluno.Nome}.");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Planos existem de forma independente dos alunos (agregação).
            Plano planoMensal = new Plano("Mensal", 120.00m);
            Plano planoAnual = new Plano("Anual", 1000.00m);

            // Cadastro dos alunos: podem existir sem nenhuma matrícula ativa.
            Aluno aluno1 = new Aluno("João Silva", "111.222.333-44", 75.5);
            Aluno aluno2 = new Aluno("Maria Souza", "999.888.777-66", 62.0);

            PersonalTrainer personal = new PersonalTrainer("Roberto");

            // Cada aluno decide por um plano diferente, gerando sua própria Matrícula.
            aluno1.Matricular(planoMensal);
            aluno2.Matricular(planoAnual);

            personal.OrientarTreino(aluno1);
            personal.OrientarTreino(aluno2);

            // Destinatário: aluno1
            // Seletor: AtualizarPeso
            // Argumento: 74.0
            aluno1.AtualizarPeso(74.0);

            try
            {
                aluno2.AtualizarPeso(-5.0);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Erro de validação capturado: {ex.Message}");
            }
        }
    }
}
