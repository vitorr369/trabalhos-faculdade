using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AprendeMais
{
    public interface IProduto
    {
        string Nome { get; }
        decimal CalcularPreco();
    }

    public interface IAceitaCupom
    {
        decimal AplicarCupom(decimal percentualDesconto);
    }

    public class Instrutor
    {
        public string Nome { get; }
        public string Especialidade { get; }

        public Instrutor(string nome, string especialidade)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("O nome do instrutor nao pode ser vazio.");
            if (string.IsNullOrWhiteSpace(especialidade))
                throw new ArgumentException("A especialidade do instrutor nao pode ser vazia.");

            Nome = nome;
            Especialidade = especialidade;
        }
    }

    public class Aluno
    {
        public string Nome { get; }
        public string Email { get; }

        public Aluno(string nome, string email)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("O nome do aluno nao pode ser vazio.");
            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
                throw new ArgumentException("Email invalido.");

            Nome = nome;
            Email = email;
        }
    }

    public class Aula
    {
        public int Numero { get; }
        public string Titulo { get; }
        public int DuracaoMinutos { get; }

        internal Aula(int numero, string titulo, int duracaoMinutos)
        {
            if (string.IsNullOrWhiteSpace(titulo))
                throw new ArgumentException("O titulo da aula nao pode ser vazio.");
            if (duracaoMinutos <= 0)
                throw new ArgumentException("A duracao deve ser maior que zero.");

            Numero = numero;
            Titulo = titulo;
            DuracaoMinutos = duracaoMinutos;
        }
    }

    public abstract class Curso : IProduto
    {
        private readonly List<Aula> _aulas = new List<Aula>();

        public string Titulo { get; }
        public int CargaHoraria { get; }
        public Instrutor Instrutor { get; }
        public IReadOnlyList<Aula> Aulas => _aulas.AsReadOnly();
        public string Nome => Titulo;

        protected Curso(string titulo, int cargaHoraria, Instrutor instrutor)
        {
            if (string.IsNullOrWhiteSpace(titulo))
                throw new ArgumentException("O titulo do curso nao pode ser vazio.");
            if (cargaHoraria <= 0)
                throw new ArgumentException("A carga horaria deve ser maior que zero.");
            if (instrutor == null)
                throw new ArgumentNullException(nameof(instrutor), "O curso precisa ter um instrutor.");

            Titulo = titulo;
            CargaHoraria = cargaHoraria;
            Instrutor = instrutor;
        }

        public Aula AdicionarAula(string titulo, int duracaoMinutos)
        {
            var aula = new Aula(_aulas.Count + 1, titulo, duracaoMinutos);
            _aulas.Add(aula);
            return aula;
        }

        public abstract decimal CalcularPreco();

        protected static decimal CalcularDesconto(decimal valor, decimal percentual)
        {
            if (percentual <= 0 || percentual > 50)
                throw new ArgumentException("O desconto deve ficar entre 1% e 50%.");

            return Math.Round(valor * (1 - percentual / 100m), 2);
        }
    }

    public class CursoGravado : Curso, IAceitaCupom
    {
        private readonly decimal _precoFixo;

        public CursoGravado(string titulo, int cargaHoraria, Instrutor instrutor, decimal precoFixo)
            : base(titulo, cargaHoraria, instrutor)
        {
            if (precoFixo <= 0)
                throw new ArgumentException("O preco precisa ser maior que zero.");

            _precoFixo = precoFixo;
        }

        public override decimal CalcularPreco()
        {
            return _precoFixo;
        }

        public decimal AplicarCupom(decimal percentualDesconto)
        {
            return CalcularDesconto(CalcularPreco(), percentualDesconto);
        }
    }

    public class CursoAoVivo : Curso, IAceitaCupom
    {
        public int QuantidadeEncontros { get; }
        public decimal ValorPorEncontro { get; }

        public CursoAoVivo(string titulo, int cargaHoraria, Instrutor instrutor, int quantidadeEncontros, decimal valorPorEncontro)
            : base(titulo, cargaHoraria, instrutor)
        {
            if (quantidadeEncontros <= 0)
                throw new ArgumentException("A quantidade de encontros deve ser maior que zero.");
            if (valorPorEncontro <= 0)
                throw new ArgumentException("O valor por encontro deve ser maior que zero.");

            QuantidadeEncontros = quantidadeEncontros;
            ValorPorEncontro = valorPorEncontro;
        }

        public override decimal CalcularPreco()
        {
            return QuantidadeEncontros * ValorPorEncontro;
        }

        public decimal AplicarCupom(decimal percentualDesconto)
        {
            return CalcularDesconto(CalcularPreco(), percentualDesconto);
        }
    }

    public class Mentoria : Curso
    {
        public int HorasMentoria { get; }
        public decimal ValorHora { get; }

        public Mentoria(string titulo, int cargaHoraria, Instrutor instrutor, int horasMentoria, decimal valorHora)
            : base(titulo, cargaHoraria, instrutor)
        {
            if (horasMentoria <= 0)
                throw new ArgumentException("As horas de mentoria devem ser maiores que zero.");
            if (valorHora <= 0)
                throw new ArgumentException("O valor da hora deve ser maior que zero.");

            HorasMentoria = horasMentoria;
            ValorHora = valorHora;
        }

        public override decimal CalcularPreco()
        {
            return HorasMentoria * ValorHora;
        }
    }

    public class Ebook : IProduto, IAceitaCupom
    {
        public string Titulo { get; }
        public string Autor { get; }
        private readonly decimal _preco;

        public string Nome => $"E-book: {Titulo}";

        public Ebook(string titulo, string autor, decimal preco)
        {
            if (string.IsNullOrWhiteSpace(titulo))
                throw new ArgumentException("O titulo do e-book nao pode ser vazio.");
            if (string.IsNullOrWhiteSpace(autor))
                throw new ArgumentException("O autor do e-book nao pode ser vazio.");
            if (preco <= 0)
                throw new ArgumentException("O preco deve ser maior que zero.");

            Titulo = titulo;
            Autor = autor;
            _preco = preco;
        }

        public decimal CalcularPreco()
        {
            return _preco;
        }

        public decimal AplicarCupom(decimal percentualDesconto)
        {
            if (percentualDesconto <= 0 || percentualDesconto > 50)
                throw new ArgumentException("O desconto deve ficar entre 1% e 50%.");

            return Math.Round(_preco * (1 - percentualDesconto / 100m), 2);
        }
    }

    public class Matricula
    {
        private readonly HashSet<int> _aulasConcluidas = new HashSet<int>();

        public Aluno Aluno { get; }
        public Curso Curso { get; }
        public DateTime DataMatricula { get; }
        public decimal? NotaFinal { get; private set; }
        public bool CertificadoEmitido { get; private set; }

        public double Progresso
        {
            get
            {
                if (Curso.Aulas.Count == 0)
                    return 0;

                return Math.Round((double)_aulasConcluidas.Count / Curso.Aulas.Count * 100, 2);
            }
        }

        public Matricula(Aluno aluno, Curso curso)
        {
            if (aluno == null)
                throw new ArgumentNullException(nameof(aluno), "A matricula precisa de um aluno.");
            if (curso == null)
                throw new ArgumentNullException(nameof(curso), "A matricula precisa de um curso.");

            Aluno = aluno;
            Curso = curso;
            DataMatricula = DateTime.Now;
        }

        public void ConcluirAula(int numeroAula)
        {
            if (Curso.Aulas.Count == 0)
                throw new InvalidOperationException("O curso nao tem aulas cadastradas.");

            if (!Curso.Aulas.Any(a => a.Numero == numeroAula))
                throw new ArgumentException($"A aula {numeroAula} nao existe neste curso.");

            if (_aulasConcluidas.Contains(numeroAula))
                throw new InvalidOperationException($"A aula {numeroAula} ja foi concluida.");

            _aulasConcluidas.Add(numeroAula);
        }

        public void RegistrarNotaFinal(decimal nota)
        {
            if (Progresso < 100)
                throw new InvalidOperationException("Precisa concluir 100% das aulas antes de registrar a nota.");
            if (nota < 0 || nota > 10)
                throw new ArgumentException("A nota deve ser entre 0 e 10.");
            if (CertificadoEmitido)
                throw new InvalidOperationException("Nao da para alterar a nota depois do certificado emitido.");

            NotaFinal = nota;
        }

        public void EmitirCertificado()
        {
            if (Progresso < 100)
                throw new InvalidOperationException("O aluno ainda nao concluiu todas as aulas.");
            if (NotaFinal == null || NotaFinal < 7.0m)
                throw new InvalidOperationException("A nota minima para certificado e 7,0.");

            CertificadoEmitido = true;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var ptBr = CultureInfo.GetCultureInfo("pt-BR");

            Console.WriteLine("=== Plataforma Aprende+ ===\n");

            var instrutor = new Instrutor("Carlos Mendes", "Engenharia de Software e C#");

            var cursoGravado = new CursoGravado("C# Orientado a Objetos", 20, instrutor, 199.90m);
            var cursoAoVivo = new CursoAoVivo("Arquitetura Limpa na Pratica", 16, instrutor, 8, 45.00m);
            var mentoria = new Mentoria("Mentoria Carreira .NET", 6, instrutor, 6, 120.00m);

            Console.WriteLine("Cursos cadastrados:");
            var cursos = new List<Curso> { cursoGravado, cursoAoVivo, mentoria };
            foreach (var c in cursos)
            {
                Console.WriteLine($"- {c.Titulo} | Instrutor: {c.Instrutor.Nome} | Preco: {c.CalcularPreco().ToString("C", ptBr)}");
            }

            cursos.Remove(cursoAoVivo);
            Console.WriteLine($"\nCurso '{cursoAoVivo.Titulo}' removido do catalogo. Instrutor {instrutor.Nome} segue ativo na plataforma.\n");

            cursoGravado.AdicionarAula("Introducao a POO", 25);
            cursoGravado.AdicionarAula("Encapsulamento e Propriedades", 30);
            cursoGravado.AdicionarAula("Heranca e Polimorfismo", 40);
            cursoGravado.AdicionarAula("Interfaces", 35);

            var aluno = new Aluno("Vitor Sidrim", "vitor@aprendemais.com");
            var matricula = new Matricula(aluno, cursoGravado);

            Console.WriteLine($"Matricula de {matricula.Aluno.Nome} em '{matricula.Curso.Titulo}' (Progresso: {matricula.Progresso}%)");
            for (int i = 1; i <= cursoGravado.Aulas.Count; i++)
            {
                matricula.ConcluirAula(i);
                Console.WriteLine($"Aula {i} concluida - Progresso: {matricula.Progresso}%");
            }

            matricula.RegistrarNotaFinal(9.5m);
            matricula.EmitirCertificado();
            Console.WriteLine($"Nota final: {matricula.NotaFinal} | Certificado emitido: {(matricula.CertificadoEmitido ? "Sim" : "Nao")}\n");

            var ebook = new Ebook("Guia Pratico de Git e GitHub", "Ana Paula Rocha", 49.90m);
            Console.WriteLine($"Venda realizada: {ebook.Nome} ({ebook.Autor}) - {ebook.CalcularPreco().ToString("C", ptBr)}\n");

            Console.WriteLine("Aplicando cupom de 15% nos produtos:");
            var produtos = new List<IProduto> { cursoGravado, cursoAoVivo, mentoria, ebook };
            foreach (var item in produtos)
            {
                if (item is IAceitaCupom comCupom)
                {
                    decimal valorFinal = comCupom.AplicarCupom(15m);
                    Console.WriteLine($"- {item.Nome}: aceitou cupom (de {item.CalcularPreco().ToString("C", ptBr)} por {valorFinal.ToString("C", ptBr)})");
                }
                else
                {
                    Console.WriteLine($"- {item.Nome}: nao aceita cupom (valor mantido em {item.CalcularPreco().ToString("C", ptBr)})");
                }
            }

            Console.WriteLine("\nTestando regras bloqueadas:");

            try
            {
                var matriculaInvalida = new Matricula(null!, cursoGravado);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro capturado (matricula sem aluno): {ex.Message}");
            }

            try
            {
                matricula.ConcluirAula(1);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro capturado (aula repetida / progresso > 100%): {ex.Message}");
            }

            try
            {
                var outraMatricula = new Matricula(aluno, cursoAoVivo);
                outraMatricula.EmitirCertificado();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro capturado (certificado sem concluir curso): {ex.Message}");
            }

            try
            {
                var cursoInvalido = new CursoGravado("Curso Teste", 10, instrutor, -50m);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro capturado (preco negativo): {ex.Message}");
            }
        }
    }
}
