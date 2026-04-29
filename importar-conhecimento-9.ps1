$baseUrl = "http://localhost:5205/conhecimento"

$dados = @(

@{
    conteudo = "O acompanhamento de dados por lote permite avaliar o desempenho real do cultivo ao longo do tempo."
    tags = "lote,monitoramento,desempenho"
    clienteId = 1
},
@{
    conteudo = "O controle de biomassa é essencial para ajustar alimentação e manejo do sistema."
    tags = "biomassa,controle,manejo"
    clienteId = 1
},
@{
    conteudo = "O peso médio deve ser monitorado regularmente para avaliar o crescimento dos organismos."
    tags = "peso medio,crescimento,monitoramento"
    clienteId = 1
},
@{
    conteudo = "A taxa de sobrevivência deve ser acompanhada para identificar perdas ao longo do cultivo."
    tags = "sobrevivencia,mortalidade,lote"
    clienteId = 1
},
@{
    conteudo = "O consumo de ração deve ser registrado para cálculo da conversão alimentar."
    tags = "racao,consumo,conversao alimentar"
    clienteId = 1
},
@{
    conteudo = "A conversão alimentar indica a eficiência do uso da ração no crescimento dos organismos."
    tags = "conversao alimentar,eficiencia"
    clienteId = 1
},
@{
    conteudo = "O acompanhamento diário permite identificar rapidamente desvios no desempenho produtivo."
    tags = "monitoramento diario,desvios"
    clienteId = 1
},
@{
    conteudo = "A análise de dados históricos de lotes melhora a tomada de decisão em ciclos futuros."
    tags = "dados historicos,decisao"
    clienteId = 1
},
@{
    conteudo = "A biomassa total influencia diretamente o consumo de oxigênio e a demanda por aeração."
    tags = "biomassa,oxigenio,aeracao"
    clienteId = 1
},
@{
    conteudo = "O aumento da biomassa exige ajustes no manejo para manter a qualidade da água."
    tags = "biomassa,qualidade agua,manejo"
    clienteId = 1
},
@{
    conteudo = "A alimentação deve ser ajustada com base no peso médio e na biomassa total."
    tags = "alimentacao,biomassa,peso medio"
    clienteId = 1
},
@{
    conteudo = "O registro de mortalidade ajuda a identificar problemas sanitários ou de manejo."
    tags = "mortalidade,sanidade"
    clienteId = 1
},
@{
    conteudo = "A eficiência produtiva pode ser avaliada pela relação entre crescimento, consumo de ração e sobrevivência."
    tags = "eficiencia,producao"
    clienteId = 1
},
@{
    conteudo = "O acompanhamento de indicadores ao longo do tempo permite ajustes contínuos no sistema."
    tags = "indicadores,monitoramento"
    clienteId = 1
},
@{
    conteudo = "A variabilidade entre lotes pode indicar inconsistências no manejo ou nas condições ambientais."
    tags = "lotes,variabilidade"
    clienteId = 1
},
@{
    conteudo = "A análise de desempenho por lote permite comparar resultados entre ciclos produtivos."
    tags = "comparacao,lotes"
    clienteId = 1
},
@{
    conteudo = "O controle detalhado de dados melhora a previsibilidade da produção."
    tags = "previsibilidade,dados"
    clienteId = 1
},
@{
    conteudo = "A tomada de decisão deve ser baseada em indicadores reais coletados durante o cultivo."
    tags = "decisao,dados reais"
    clienteId = 1
},
@{
    conteudo = "O acompanhamento do lote permite identificar o momento ideal para ajustes de manejo."
    tags = "lote,manejo,ajuste"
    clienteId = 1
},
@{
    conteudo = "A integração de dados operacionais e produtivos aumenta a eficiência do sistema."
    tags = "integracao dados,eficiencia"
    clienteId = 1
}

)

foreach ($item in $dados) {

    $json = $item | ConvertTo-Json -Depth 3

    try {
        Invoke-RestMethod -Uri $baseUrl `
            -Method Post `
            -ContentType "application/json" `
            -Body $json

        Write-Host "OK: $($item.conteudo.Substring(0,40))..."
    }
    catch {
        Write-Host "ERRO ao enviar item"
    }
}