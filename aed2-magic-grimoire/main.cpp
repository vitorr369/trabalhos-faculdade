#include <iostream>
#include <vector>
#include <string>
#include <cstdlib>
#include <ctime>
#include <iomanip>
#include <locale.h>

using namespace std;

// ==========================================
// ESTRUTURA DA FICHA (D&D 5e)
// ==========================================
struct Ficha {
    string nome;
    string raca;
    string classe;
    int nivel; // Nivel do personagem (1 ao 10)
    
    // Atributos
    int forca;
    int destreza;
    int constituicao;
    int inteligencia;
    int sabedoria;
    int carisma;
    
    // Grimorio (Magias e Habilidades aprendidas)
    vector<string> magiasConhecidas;
};

vector<Ficha> meusPersonagens;

// ==========================================
// UTILITÁRIOS
// ==========================================
int rolarAtributo() {
    // Rola 3 dados de 6 lados (Gera entre 3 e 18)
    return (rand() % 6 + 1) + (rand() % 6 + 1) + (rand() % 6 + 1);
}

// ==========================================
// BANCO DE DADOS DE MAGIAS E PROGRESSÃO
// ==========================================
vector<string> obterMagiasDaClasse(string classe, int nivel) {
    vector<string> lista;
    
    // --- MAGO / FEITICEIRO ---
    if (classe == "Mago" || classe == "Feiticeiro") {
        lista.push_back("[Truque] Raio de Fogo");
        lista.push_back("[Truque] Maos Magicas");
        lista.push_back("[Truque] Luz");
        lista.push_back("[C1] Misseis Magicos");
        lista.push_back("[C1] Escudo Arcano");
        lista.push_back("[C1] Sono");
        lista.push_back("[C1] Onda Trovejante");
        
        if (nivel >= 3) {
            lista.push_back("[C2] Passo Nebuloso (Misty Step)");
            lista.push_back("[C2] Invisibilidade");
            lista.push_back("[C2] Raio Ardente");
            lista.push_back("[C2] Teia");
        }
        if (nivel >= 5) {
            lista.push_back("[C3] Bola de Fogo (Fireball)");
            lista.push_back("[C3] Contra-Magica");
            lista.push_back("[C3] Voo");
            lista.push_back("[C3] Relampago");
        }
        if (nivel >= 7) {
            lista.push_back("[C4] Polimorfia");
            lista.push_back("[C4] Porta Dimensional");
            lista.push_back("[C4] Banimento");
        }
        if (nivel >= 9) {
            lista.push_back("[C5] Cone de Frio");
            lista.push_back("[C5] Muralha de Forca");
            lista.push_back("[C5] Dominar Pessoa");
        }
    } 
    // --- CLERIGO ---
    else if (classe == "Clerigo") {
        lista.push_back("[Truque] Chama Sagrada");
        lista.push_back("[Truque] Orientacao");
        lista.push_back("[C1] Curar Ferimentos");
        lista.push_back("[C1] Bencao (Bless)");
        lista.push_back("[C1] Raio Guiador");
        lista.push_back("[C1] Escudo da Fe");
        
        if (nivel >= 3) {
            lista.push_back("[C2] Arma Espiritual");
            lista.push_back("[C2] Restauracao Menor");
            lista.push_back("[C2] Silencio");
        }
        if (nivel >= 5) {
            lista.push_back("[C3] Espiritos Guardioes");
            lista.push_back("[C3] Revivificar");
            lista.push_back("[C3] Palavra Curativa em Massa");
        }
        if (nivel >= 7) {
            lista.push_back("[C4] Banimento");
            lista.push_back("[C4] Protecao contra a Morte");
        }
        if (nivel >= 9) {
            lista.push_back("[C5] Curar Ferimentos em Massa");
            lista.push_back("[C5] Chama Sagrada (Flame Strike)");
            lista.push_back("[C5] Reviver os Mortos");
        }
    }
    // --- DRUIDA ---
    else if (classe == "Druida") {
        lista.push_back("[Truque] Bordao Mistico");
        lista.push_back("[Truque] Druidismo");
        lista.push_back("[Truque] Chicote de Espinhos");
        lista.push_back("[C1] Constricao");
        lista.push_back("[C1] Bom Fruto");
        lista.push_back("[C1] Falar com Animais");
        
        if (nivel >= 2) lista.push_back("[Hab] Forma Selvagem (Wild Shape)");
        
        if (nivel >= 3) {
            lista.push_back("[C2] Pele de Arvore");
            lista.push_back("[C2] Raio Lunar");
            lista.push_back("[C2] Passos sem Pegadas");
        }
        if (nivel >= 5) {
            lista.push_back("[C3] Conjurar Animais");
            lista.push_back("[C3] Convocar Relampagos");
        }
        if (nivel >= 7) {
            lista.push_back("[C4] Pele de Pedra");
            lista.push_back("[C4] Tempestade de Gelo");
        }
        if (nivel >= 9) {
            lista.push_back("[C5] Reencarnacao");
            lista.push_back("[C5] Praga de Insetos");
        }
    }
    // --- BARDO ---
    else if (classe == "Bardo") {
        lista.push_back("[Truque] Zombaria Viciosa");
        lista.push_back("[Hab] Inspiracao de Bardo");
        lista.push_back("[C1] Curar Ferimentos");
        lista.push_back("[C1] Riso Hediondo");
        lista.push_back("[C1] Sussurros Dissonantes");
        
        if (nivel >= 3) {
            lista.push_back("[C2] Sugestao");
            lista.push_back("[C2] Despedacar");
            lista.push_back("[C2] Invisibilidade");
        }
        if (nivel >= 5) {
            lista.push_back("[Hab] Fonte de Inspiracao");
            lista.push_back("[C3] Padrao Hipnotico");
            lista.push_back("[C3] Dissipar Magia");
        }
        if (nivel >= 7) {
            lista.push_back("[C4] Invisibilidade Maior");
            lista.push_back("[C4] Polimorfia");
        }
        if (nivel >= 9) {
            lista.push_back("[C5] Dominar Pessoa");
            lista.push_back("[C5] Imobilizar Monstro");
        }
    }
    // --- BRUXO (WARLOCK) ---
    else if (classe == "Bruxo") {
        lista.push_back("[Truque] Rajada Mistica (Eldritch Blast)");
        lista.push_back("[C1] Bruxaria (Hex)");
        lista.push_back("[C1] Repreensao Infernal");
        
        if (nivel >= 3) {
            lista.push_back("[C2] Escuridao");
            lista.push_back("[C2] Passo Nebuloso");
        }
        if (nivel >= 5) {
            lista.push_back("[C3] Bola de Fogo");
            lista.push_back("[C3] Voo");
        }
        if (nivel >= 7) lista.push_back("[C4] Banimento");
        if (nivel >= 9) lista.push_back("[C5] Contato Extraplanar");
    }
    // --- PALADINO ---
    else if (classe == "Paladino") {
        lista.push_back("[Hab] Sentido Divino");
        lista.push_back("[Hab] Cura pelas Maos");
        
        if (nivel >= 2) {
            lista.push_back("[Hab] Punicao Divina (Smite)");
            lista.push_back("[C1] Bencao");
            lista.push_back("[C1] Heroismo");
            lista.push_back("[C1] Duelo Compelido");
        }
        if (nivel >= 5) {
            lista.push_back("[Hab] ATAQUE EXTRA");
            lista.push_back("[C2] Montaria Findal");
            lista.push_back("[C2] Ajuda");
        }
        if (nivel >= 9) {
            lista.push_back("[C3] Aura de Vitalidade");
            lista.push_back("[C3] Revivificar");
        }
    }
    // --- PATRULHEIRO (RANGER) ---
    else if (classe == "Patrulheiro") {
        lista.push_back("[Hab] Inimigo Favorito");
        
        if (nivel >= 2) {
            lista.push_back("[C1] Marca do Cacador");
            lista.push_back("[C1] Curar Ferimentos");
            lista.push_back("[C1] Saraivada de Espinhos");
        }
        if (nivel >= 5) {
            lista.push_back("[Hab] ATAQUE EXTRA");
            lista.push_back("[C2] Passos sem Pegadas");
            lista.push_back("[C2] Silencio");
        }
        if (nivel >= 9) {
            lista.push_back("[C3] Conjurar Animais");
            lista.push_back("[C3] Flecha Relampago");
        }
    }
    // --- LADINO ---
    else if (classe == "Ladino") {
        lista.push_back("[Hab] Ataque Furtivo");
        lista.push_back("[Hab] Giria de Ladrao");
        if (nivel >= 2) lista.push_back("[Hab] Acao Astuta");
        if (nivel >= 3) lista.push_back("[Hab] Arquetipo Ladino");
        if (nivel >= 5) lista.push_back("[Hab] Esquiva Sobrenatural");
        if (nivel >= 7) lista.push_back("[Hab] Evasao");
    }
    // --- BARBARO ---
    else if (classe == "Barbaro") {
        lista.push_back("[Hab] Furia");
        lista.push_back("[Hab] Defesa sem Armadura");
        if (nivel >= 2) lista.push_back("[Hab] Ataque Descuidado");
        if (nivel >= 5) lista.push_back("[Hab] ATAQUE EXTRA");
        if (nivel >= 5) lista.push_back("[Hab] Movimento Rapido");
        if (nivel >= 9) lista.push_back("[Hab] Critico Brutal");
    }
    // --- MONGE ---
    else if (classe == "Monge") {
        lista.push_back("[Hab] Artes Marciais");
        if (nivel >= 2) lista.push_back("[Hab] Ki (Pontos de Ki)");
        if (nivel >= 3) lista.push_back("[Hab] Defletir Misseis");
        if (nivel >= 5) lista.push_back("[Hab] ATAQUE EXTRA");
        if (nivel >= 5) lista.push_back("[Hab] Golpe Atordoante");
    }
    // --- GUERREIRO (E Default) ---
    else {
        lista.push_back("[Hab] Retomada de Folego");
        lista.push_back("[Hab] Estilo de Combate");
        if (nivel >= 2) lista.push_back("[Hab] Surto de Acao (Action Surge)");
        if (nivel >= 5) lista.push_back("[Hab] ATAQUE EXTRA");
        if (nivel >= 9) lista.push_back("[Hab] Indomavel");
    }
    
    return lista;
}

void gerenciarMagias(Ficha &f) {
    vector<string> disponiveis = obterMagiasDaClasse(f.classe, f.nivel);
    
    cout << "\n=== GRIMORIO (" << f.classe << " - Nivel " << f.nivel << ") ===" << endl;
    cout << "Legenda: [C1] = Magia Circulo 1 | [Hab] = Habilidade" << endl;
    
    while(true) {
        vector<string> paraAprender;
        
        for (size_t i = 0; i < disponiveis.size(); i++) {
            bool jaTem = false;
            for(size_t k=0; k < f.magiasConhecidas.size(); k++) {
                if(f.magiasConhecidas[k] == disponiveis[i]) jaTem = true;
            }
            if(!jaTem) paraAprender.push_back(disponiveis[i]);
        }
        
        if(paraAprender.empty()) {
            cout << "\n[INFO] Voce ja aprendeu tudo disponivel para seu nivel!" << endl;
            break;
        }

        cout << "\n--- Disponiveis para aprender: ---" << endl;
        for (size_t i = 0; i < paraAprender.size(); i++) {
            cout << i + 1 << ". " << paraAprender[i] << endl;
        }
        cout << "0. Sair do Grimorio" << endl;
        
        int escolha;
        cout << "Escolha o numero: ";
        cin >> escolha;
        
        if (escolha == 0) break;
        
        if (escolha > 0 && escolha <= paraAprender.size()) {
            f.magiasConhecidas.push_back(paraAprender[escolha - 1]);
            cout << ">> SUCESSO! Voce aprendeu: " << paraAprender[escolha - 1] << endl;
        }
    }
}

// ==========================================
// MENUS DE SELEÇÃO (RAÇA E CLASSE)
// ==========================================
string menuRacas() {
    cout << "\n--- ESCOLHA A RACA ---" << endl;
    cout << "1. Anao       2. Elfo       3. Halfling" << endl;
    cout << "4. Humano     5. Draconato  6. Gnomo" << endl;
    cout << "7. Meio-Elfo  8. Meio-Orc   9. Tiefling" << endl;
    cout << "Opcao: ";
    int op; cin >> op;
    switch(op) {
        case 1: return "Anao"; case 2: return "Elfo"; case 3: return "Halfling";
        case 4: return "Humano"; case 5: return "Draconato"; case 6: return "Gnomo";
        case 7: return "Meio-Elfo"; case 8: return "Meio-Orc"; case 9: return "Tiefling";
        default: return "Humano";
    }
}

string menuClasses() {
    cout << "\n--- ESCOLHA A CLASSE ---" << endl;
    cout << "1. Barbaro    2. Bardo      3. Clerigo" << endl;
    cout << "4. Druida     5. Guerreiro  6. Monge" << endl;
    cout << "7. Paladino   8. Patrulheiro 9. Ladino" << endl;
    cout << "10. Feiticeiro 11. Bruxo    12. Mago" << endl;
    cout << "Opcao: ";
    int op; cin >> op;
    switch(op) {
        case 1: return "Barbaro"; case 2: return "Bardo"; case 3: return "Clerigo";
        case 4: return "Druida"; case 5: return "Guerreiro"; case 6: return "Monge";
        case 7: return "Paladino"; case 8: return "Patrulheiro"; case 9: return "Ladino";
        case 10: return "Feiticeiro"; case 11: return "Bruxo"; case 12: return "Mago";
        default: return "Guerreiro";
    }
}

// ==========================================
// FUNÇÕES PRINCIPAIS (CRUD)
// ==========================================
void criarNovaFicha() {
    Ficha nova;
    int escolha;

    cout << "\n=== CRIACAO DE PERSONAGEM (Nv 1) ===" << endl;
    cin.ignore(); 
    
    cout << "Nome do Heroi: "; 
    getline(cin, nova.nome);
    
    nova.raca = menuRacas();
    nova.classe = menuClasses();
    nova.nivel = 1; // Personagens nascem nivel 1

    cout << "\n[1] Rolar Dados (3d6) ou [2] Manual? ";
    cin >> escolha;

    if (escolha == 1) {
        cout << "Rolando dados do destino..." << endl;
        nova.forca = rolarAtributo(); nova.destreza = rolarAtributo();
        nova.constituicao = rolarAtributo(); nova.inteligencia = rolarAtributo();
        nova.sabedoria = rolarAtributo(); nova.carisma = rolarAtributo();
        cout << "Atributos definidos!" << endl;
    } else {
        cout << "Digite os valores (FOR DES CON INT SAB CAR): ";
        cin >> nova.forca >> nova.destreza >> nova.constituicao 
            >> nova.inteligencia >> nova.sabedoria >> nova.carisma;
    }
    
    cout << "\nDeseja escolher magias/habilidades iniciais? (1-Sim / 0-Nao): ";
    int q; cin >> q;
    if(q==1) gerenciarMagias(nova);

    meusPersonagens.push_back(nova);
    cout << "\n[SUCESSO] Personagem criado!" << endl;
    system("pause");
}

void subirNivel() {
    if (meusPersonagens.empty()) {
        cout << "\nNenhum personagem criado ainda." << endl;
        system("pause");
        return;
    }
    
    cout << "\n=== SUBIR DE NIVEL (LEVEL UP) ===" << endl;
    cout << "Quem vai evoluir?" << endl;
    for(size_t i=0; i<meusPersonagens.size(); i++) 
        cout << i << ". " << meusPersonagens[i].nome << " (" << meusPersonagens[i].classe << " Nv " << meusPersonagens[i].nivel << ")" << endl;
    
    int id; 
    cout << "Digite o ID: "; cin >> id;
    
    if(id >= 0 && id < meusPersonagens.size()) {
        if (meusPersonagens[id].nivel < 10) {
            meusPersonagens[id].nivel++;
            cout << "\n!!! LEVEL UP !!!" << endl;
            cout << meusPersonagens[id].nome << " alcancou o NIVEL " << meusPersonagens[id].nivel << "!" << endl;
            
            cout << "Abrir Grimorio para ver novas habilidades? (1-Sim / 0-Nao): ";
            int q; cin >> q;
            if (q == 1) gerenciarMagias(meusPersonagens[id]);
        } else {
            cout << "Este personagem ja esta no nivel maximo (10)!" << endl;
        }
    }
    system("pause");
}

void mostrarFicha(Ficha f) {
    cout << "\n-------------------------------------------------" << endl;
    cout << " NOME: " << f.nome << endl;
    cout << " DETALHES: " << f.raca << " " << f.classe << " (Nivel " << f.nivel << ")" << endl;
    cout << "-------------------------------------------------" << endl;
    cout << " FOR: " << f.forca << " | DES: " << f.destreza << " | CON: " << f.constituicao << endl;
    cout << " INT: " << f.inteligencia << " | SAB: " << f.sabedoria << " | CAR: " << f.carisma << endl;
    cout << "-------------------------------------------------" << endl;
    cout << " HABILIDADES & MAGIAS CONHECIDAS: " << endl;
    if (f.magiasConhecidas.empty()) cout << "  (Nenhuma aprendida ainda)";
    else {
        for(size_t i=0; i<f.magiasConhecidas.size(); i++) cout << "  * " << f.magiasConhecidas[i] << endl;
    }
    cout << "-------------------------------------------------" << endl;
}

// ==========================================
// ALGORITMOS OBRIGATÓRIOS DO TRABALHO
// ==========================================

// 1. ORDENAÇÃO (Bubble Sort)
void ordenarPorForca() {
    if (meusPersonagens.empty()) return;
    vector<Ficha> lista = meusPersonagens; // Copia para não alterar a ordem original de IDs
    int n = lista.size();
    
    for (int i = 0; i < n - 1; i++) {
        for (int j = 0; j < n - i - 1; j++) {
            // Ordena do Maior para o Menor
            if (lista[j].forca < lista[j + 1].forca) { 
                Ficha temp = lista[j];
                lista[j] = lista[j + 1];
                lista[j + 1] = temp;
            }
        }
    }
    cout << "\n=== RANKING DE FORCA (Mais Forte para Mais Fraco) ===" << endl;
    for (size_t i=0; i<lista.size(); i++) mostrarFicha(lista[i]);
    system("pause");
}

// 2. BUSCA (Linear Search)
void buscarPersonagem() {
    string nome;
    cout << "\nDigite o nome do personagem: "; 
    cin.ignore(); getline(cin, nome);
    
    bool achou = false;
    for(size_t i=0; i<meusPersonagens.size(); i++) {
        if(meusPersonagens[i].nome == nome) {
            mostrarFicha(meusPersonagens[i]); 
            achou = true;
        }
    }
    if(!achou) cout << "\n[ERRO] Personagem nao encontrado." << endl;
    system("pause");
}

// 3. SELEÇÃO (Finding Max)
void selecionarMagoMaisForte() {
    if(meusPersonagens.empty()) {
        cout << "Nenhum personagem cadastrado." << endl;
        system("pause");
        return;
    }
    
    Ficha mvp = meusPersonagens[0];
    for(size_t i=1; i<meusPersonagens.size(); i++) {
        // Seleciona baseado em Inteligencia (Atributo magico principal)
        if(meusPersonagens[i].inteligencia > mvp.inteligencia) mvp = meusPersonagens[i];
    }
    
    cout << "\n=== MAGO MAIS FORTE (Maior Inteligencia) ===" << endl;
    mostrarFicha(mvp);
    system("pause");
}

void menuMagias() {
    if (meusPersonagens.empty()) {
        cout << "Crie um personagem primeiro." << endl;
        system("pause");
        return;
    }
    cout << "\nEscolha o personagem para abrir o Grimorio:" << endl;
    for(size_t i=0; i<meusPersonagens.size(); i++) 
        cout << i << ". " << meusPersonagens[i].nome << " (" << meusPersonagens[i].classe << ")" << endl;
    
    int id; 
    cout << "ID: "; cin >> id;
    if(id >= 0 && id < meusPersonagens.size()) gerenciarMagias(meusPersonagens[id]);
}

// ==========================================
// MAIN (LOOP PRINCIPAL)
// ==========================================
int main() {
    srand(time(0)); // Inicializa semente aleatoria
    setlocale(LC_ALL, "Portuguese"); // Permite acentos

    int opcao = -1;
    while (opcao != 0) {
        system("cls");
        cout << "===== Online Magic Grimoire =====" << endl;
        cout << "1. Criar Personagem" << endl;
        cout << "2. Ver Todos os Personagens" << endl;
        cout << "3. Buscar Personagem" << endl;
        cout << "4. Ranking de Forca" << endl;
        cout << "5. Mago Mais Forte" << endl;
        cout << "6. Ensinar Magias/Habilidades" << endl;
        cout << "7. SUBIR DE NIVEL (Level Up!)" << endl;
        cout << "0. Sair" << endl;
        cout << "--------------------------------------" << endl;
        cout << "Escolha uma opcao: ";
        cin >> opcao;

        switch(opcao) {
            case 1: criarNovaFicha(); break;
            case 2: 
                if(meusPersonagens.empty()) cout << "\nLista vazia." << endl;
                for(size_t i=0; i<meusPersonagens.size(); i++) mostrarFicha(meusPersonagens[i]); 
                system("pause"); 
                break;
            case 3: buscarPersonagem(); break;
            case 4: ordenarPorForca(); break;
            case 5: selecionarMagoMaisForte(); break;
            case 6: menuMagias(); break;
            case 7: subirNivel(); break;
            case 0: cout << "Saindo do sistema..." << endl; break;
            default: cout << "Opcao invalida!" << endl; system("pause"); break;
        }
    }
    return 0;
}
