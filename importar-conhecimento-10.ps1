$baseUrl = "http://localhost:5205/conhecimento"

$dados = @(
    @{ conteudo = "O monitoramento contínuo do cultivo identifica desvios de desempenho e permite ações corretivas imediatas."; tags = "monitoramento,cultivo,desempenho"; clienteId = 1 },
    @{ conteudo = "Peso médio, biomassa total e taxa de conversão alimentar são indicadores-chave de eficiência no sistema de cultivo."; tags = "peso medio,biomassa,conversao alimentar,indicadores"; clienteId = 1 },
    @{ conteudo = "A coleta de dados por lote possibilita analisar o desempenho específico de cada grupo de animais."; tags = "dados,lote,analise"; clienteId = 1 },
    @{ conteudo = "Registros diários de variáveis são essenciais; Embrapa recomenda anotar diariamente consumo de ração, mortalidade e parâmetros de água【18†L7-L10】."; tags = "registro,diario,qualidade agua"; clienteId = 1 },
    @{ conteudo = "Dados históricos de ciclos anteriores ajudam a projetar resultados futuros e comparar desempenho."; tags = "dados historicos,projecoes,comparacao"; clienteId = 1 },
    @{ conteudo = "A análise de tendências mostra padrões de crescimento e alerta sobre quedas na produtividade."; tags = "tendencias,analise,produtividade"; clienteId = 1 },
    @{ conteudo = "Alertas operacionais podem ser acionados quando indicadores críticos ultrapassam limites seguros."; tags = "alertas,operacionais,indicadores"; clienteId = 1 },
    @{ conteudo = "Integrar indicadores técnicos com KPIs financeiros fortalece a gestão e a tomada de decisão."; tags = "integracao,financeiro,gestao"; clienteId = 1 },
    @{ conteudo = "Na fase juvenil, priorize menor densidade e alimentação balanceada para maximizar a sobrevivência."; tags = "fase:juvenil,densidade,alimentacao"; clienteId = 1 },
    @{ conteudo = "Na fase de engorda, foque no ganho de peso e otimização da alimentação até o ponto de abate."; tags = "fase:engorda,ganho peso,alimentacao"; clienteId = 1 },
    @{ conteudo = "Manter alta qualidade da água (oxigênio, amônia, pH) é crucial em todas as fases do cultivo."; tags = "qualidade agua,oxigenio,ph"; clienteId = 1 },
    @{ conteudo = "Controlar o consumo de ração é vital para calcular a conversão alimentar e ajustar dietas."; tags = "consumo,racao,conversao alimentar"; clienteId = 1 },
    @{ conteudo = "Planejamento de colheita considera o peso ideal de mercado e a logística de abate."; tags = "colheita,planejamento,logistica"; clienteId = 1 },
    @{ conteudo = "Scripts de importação devem ser versionados em repositório e ter backup frequente."; tags = "backup,scripts,versao"; clienteId = 1 },
    @{ conteudo = "Consolidar múltiplos scripts em um único seed (.NET) simplifica a manutenção do sistema."; tags = "seed,consolidacao,manutencao"; clienteId = 1 },
    @{ conteudo = "Sugestão de tags: tema:monitoramento, fase:juvenil, tipo:operacional para categorizar o conhecimento."; tags = "tema:monitoramento,fase:juvenil,tipo:operacional"; clienteId = 1 },
    @{ conteudo = "A densidade de estocagem impacta diretamente a biomassa final e o consumo de oxigênio."; tags = "densidade,biomassa,oxigenio"; clienteId = 1 },
    @{ conteudo = "Integração de dados operacionais com indicadores econômicos maximiza a rentabilidade."; tags = "integracao,economico,rentabilidade"; clienteId = 1 },
    @{ conteudo = "Simulações de cenários utilizam indicadores atuais para prever lucros ou prejuízos."; tags = "simulacao,cenario,indicadores"; clienteId = 1 },
    @{ conteudo = "Documentar processos e scripts garante reprodutibilidade e facilita auditorias."; tags = "documentacao,auditoria,reprodutibilidade"; clienteId = 1 },
    @{ conteudo = "Mortalidade elevada sinaliza problemas de sanidade ou manejo inadequado."; tags = "mortalidade,sanidade,gestao"; clienteId = 1 },
    @{ conteudo = "Taxa de sobrevivência acima de 90% é desejável para garantir viabilidade econômica."; tags = "sobrevivencia,viabilidade,economico"; clienteId = 1 },
    @{ conteudo = "Gráficos de tendência dos indicadores facilitam a visualização do desempenho do cultivo."; tags = "grafico,tendencia,indicadores"; clienteId = 1 },
    @{ conteudo = "Ajustes na alimentação e densidade devem ser realizados conforme a análise de dados coletados."; tags = "analise,dados,ajustes"; clienteId = 1 },
    @{ conteudo = "Armazenar backups do banco de dados e scripts aumenta a segurança dos registros."; tags = "backup,banco de dados,seguranca"; clienteId = 1 },
    @{ conteudo = "O controle do peso médio ao longo do tempo indica a performance de crescimento dos peixes."; tags = "peso medio,crescimento,indicador"; clienteId = 1 },
    @{ conteudo = "Monitorar pH e temperatura ajuda a manter condições ideais de bem-estar dos organismos."; tags = "ph,temperatura,qualidade"; clienteId = 1 },
    @{ conteudo = "Sensores de oxigênio e temperatura podem acionar alertas em tempo real em sistemas automatizados."; tags = "sensores,automacao,alertas"; clienteId = 1 },
)

foreach ($item in $dados) {
    $json = $item | ConvertTo-Json -Depth 3
    try {
        $response = Invoke-WebRequest -Uri $baseUrl `
            -Method Post `
            -ContentType "application/json" `
            -Body $json
        if ($response.StatusCode -eq 200 -or $response.StatusCode -eq 201) {
            Write-Host "OK: " $($item.conteudo.Substring(0,40)) "..."
        } else {
            Write-Host "ERRO: Status" $response.StatusCode "- Item:" $($item.conteudo.Substring(0,40)) "..."
        }
    }
    catch {
        Write-Host "ERRO: " $_.Exception.Message " - Item:" $($item.conteudo.Substring(0,40)) "..."
    }
}
