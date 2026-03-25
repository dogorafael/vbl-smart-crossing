# VBL Smart Crossing 🐸🚦

Protótipo desenvolvido como resposta ao desafio técnico **Game Developer Pleno** do Centro de Pesquisas Avançadas Wernher von Braun.

Uma releitura do clássico Frogger onde o tráfego e o clima **não são aleatórios** — eles são determinados em tempo real por uma API REST de predição de tráfego. O jogador controla um personagem que deve atravessar uma via expressa dentro de um tempo limite, enquanto as condições da via se agravam automaticamente conforme as predições da API se concretizam.

---

## Pré-requisitos

| Ferramenta | Versão utilizada |
|---|---|
| Unity | **6.3.9f1** |
| Mockoon | 9.x ou superior |
| Git | Qualquer versão recente |

> As dependências de pacotes Unity (Newtonsoft Json.NET e TextMeshPro) já estão declaradas no `Packages/manifest.json` do projeto e serão baixadas automaticamente pelo Package Manager ao abrir o projeto.

---

## 1 — Clonar o repositório

```bash
git clone https://github.com/<seu-usuario>/vbl-smart-crossing.git
cd vbl-smart-crossing
```

---

## 2 — Configurar o Mock da API (Mockoon)

O projeto inclui o arquivo de configuração pronto em `mockoon/vbl-traffic-api.json`.

**Passo a passo:**

1. Abra o **Mockoon**
2. Clique em **File → Open local environment** ou no ícone [+]
3. Selecione o arquivo `mockoon/vbl-traffic-api.json` na raiz do repositório
4. Clique em **▶ Start server**

O endpoint ficará disponível em:
```
GET http://localhost:3000/v1/traffic/status
```

O mock está configurado em modo **SEQUENTIAL** — cada chamada retorna um cenário progressivamente mais difícil:

| Chamada | Condição | Densidade | Velocidade |
|---|---|---|---|
| 1ª (Nível 1) | Ensolarado | 0.2 | 30 km/h |
| 2ª (Nível 2) | Nublado | 0.4 | 55 km/h |
| 3ª (Nível 3) | Neblina | 0.65 | 75 km/h |
| 4ª em diante | Reinicia do Nível 1 | — | — |

> **Sem o Mockoon:** o jogo possui um fallback embutido (`Assets/Resources/MockData/vbl-traffic-fallback.json`) com os mesmos 3 cenários. Se a API não responder, ele é carregado automaticamente e o jogo funciona normalmente.

---

## 3 — Abrir o projeto no Unity

1. Abra o **Unity Hub**
2. Clique em **Add → Add project from disk**
3. Selecione a pasta raiz do repositório clonado
4. Certifique-se de que o Unity Hub usa a versão **6.3.9f1** para abrir o projeto
5. Aguarde a importação dos pacotes (primeira abertura pode demorar alguns minutos)

---

## 4 — Rodar a simulação

1. No **Project** panel, abra a cena `Assets/Scenes/SmartCrossing.unity`
2. Certifique-se de que o **Mockoon está rodando** (passo 2)
3. Pressione **▶ Play**

**Controles:**

| Tecla | Ação |
|---|---|
| `W` / `↑` | Mover para frente (atravessar a via) |
| `S` / `↓` | Recuar |
| `A` / `←` | Mover para a esquerda |
| `D` / `→` | Mover para a direita |

---

## Regras da simulação

| Condição | Resultado |
|---|---|
| Jogador alcança a zona segura do outro lado | **Nível completo** — nova chamada à API carrega o próximo cenário |
| Cronômetro chega a zero | **Game Over** — o tempo é definido pelo `estimated_time` da última predição recebida |
| Jogador colide com um veículo | **Game Over** |

---

## Fórmulas implementadas

Todas as fórmulas seguem exatamente a especificação do desafio:

| Mecânica | Fórmula |
|---|---|
| Intervalo de spawn dos veículos | `1 / vehicleDensity` |
| Velocidade dos veículos na engine | `(averageSpeed / 100) × ReferenceSpeed` |
| Velocidade do jogador | `BaseSpeed × weather_multiplier` |
| Duração do cronômetro | `último predicted_status.estimated_time ÷ 1000` (ms → s) |
| Agendamento de predições | `WaitForSeconds(estimated_time / 1000f)` |

**Multiplicadores de clima:**

| Condição | Multiplicador |
|---|---|
| `sunny` | 1.0× |
| `clouded` / `foggy` | 0.8× |
| `light rain` | 0.6× |
| `heavy rain` | 0.4× |

---

## Arquitetura

O projeto adota separação estrita entre **camada de dados** e **camada de visualização**, com comunicação entre sistemas via **Event Bus**:

```
┌─────────────────────────────────────────────────────┐
│                    Game Layer                        │
│  GameManager · LevelManager · PredictionScheduler   │
│  TrafficManager · PlayerController · WeatherManager  │
│  HudController                                       │
├─────────────────────────────────────────────────────┤
│              GameEvents (Event Bus)                  │
│   Comunicação desacoplada via C# events estáticos    │
├─────────────────────────────────────────────────────┤
│                  Service Layer                       │
│       ApiService — única classe que faz HTTP         │
├─────────────────────────────────────────────────────┤
│                   Data Layer                         │
│   TrafficDataModels — espelha o contrato OpenAPI     │
└─────────────────────────────────────────────────────┘
```

**Decisões relevantes:**

- **ApiService é uma classe C# pura** (sem MonoBehaviour) — facilita substituição e teste isolado
- **Event Bus estático** — nenhum sistema conhece diretamente os outros; adicionar ou remover um sistema não quebra os demais
- **PredictionScheduler usa coroutines com `WaitForSeconds`** — sensível ao `Time.timeScale`, então pausar o jogo pausa as predições automaticamente
- **Fallback em `Resources/`** carregado sob demanda — o JSON só é lido do disco se a API falhar, e apenas uma vez (cached em memória nas chamadas seguintes)

---

