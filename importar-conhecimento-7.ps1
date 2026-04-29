$baseUrl = "http://localhost:5205/conhecimento"

$dados = @(

@{
    conteudo = "O ciclo produtivo pode ser dividido em fases como juvenil e engorda, cada uma com necessidades específicas de manejo."
    tags = "ciclo produtivo,fases,juvenil,engorda"
    clienteId = 1
},
@{
    conteudo = "A fase juvenil exige maior controle de qualidade de água e alimentação para garantir bom desenvolvimento inicial."
    tags = "juvenil,manejo inicial,qualidade agua"
    clienteId = 1
},
@{
    conteudo = "A fase de engorda é focada no ganho de peso e eficiência alimentar até o ponto de abate."
    tags = "engorda,ganho peso,abate"
    clienteId = 1
},
@{
    conteudo = "A taxa de crescimento varia conforme a fase do cultivo e as condições ambientais."
    tags = "crescimento,fatores ambientais,fases"
    clienteId = 1
},
@{
    conteudo = "O consumo de ração aumenta progressivamente ao longo do ciclo produtivo."
    tags = "racao,consumo,ciclo"
    clienteId = 1
},
@{
    conteudo = "A conversão alimentar deve ser monitorada em todas as fases para garantir eficiência produtiva."
    tags = "conversao alimentar,eficiencia,fases"
    clienteId = 1
},
@{
    conteudo = "A sobrevivência nas fases iniciais impacta diretamente o resultado final da produção."
    tags = "sobrevivencia,juvenil,impacto"
    clienteId = 1
},
@{
    conteudo = "O planejamento de povoamento deve considerar a capacidade do sistema e a densidade adequada para cada fase."
    tags = "povoamento,densidade,planejamento"
    clienteId = 1
},
@{
    conteudo = "A biomassa total aumenta ao longo do ciclo e influencia diretamente o consumo de oxigênio."
    tags = "biomassa,oxigenio,consumo"
    clienteId = 1
},
@{
    conteudo = "A demanda por oxigênio cresce com o aumento da biomassa e da atividade metabólica."
    tags = "oxigenio,biomassa,metabolismo"
    clienteId = 1
},
@{
    conteudo = "A qualidade da água deve ser ajustada continuamente conforme a evolução do cultivo."
    tags = "qualidade agua,monitoramento,ajuste"
    clienteId = 1
},
@{
    conteudo = "A alimentação deve ser ajustada conforme o peso médio dos animais e a fase produtiva."
    tags = "alimentacao,peso medio,ajuste"
    clienteId = 1
},
@{
    conteudo = "O tempo de cultivo depende da meta de peso final e das condições do sistema."
    tags = "tempo cultivo,peso final"
    clienteId = 1
},
@{
    conteudo = "A densidade de estocagem deve ser ajustada para evitar estresse e queda de desempenho."
    tags = "densidade,estresse,desempenho"
    clienteId = 1
},
@{
    conteudo = "A taxa de mortalidade deve ser monitorada para avaliar problemas de manejo ou sanidade."
    tags = "mortalidade,monitoramento,sanidade"
    clienteId = 1
},
@{
    conteudo = "O manejo alimentar inadequado pode comprometer a qualidade da água e aumentar os custos."
    tags = "alimentacao,qualidade agua,custos"
    clienteId = 1
},
@{
    conteudo = "A integração das fases permite melhor planejamento logístico e financeiro da produção."
    tags = "planejamento,ciclo completo,logistica"
    clienteId = 1
},
@{
    conteudo = "O acompanhamento do peso médio é essencial para tomada de decisão ao longo do ciclo."
    tags = "peso medio,monitoramento,decisao"
    clienteId = 1
},
@{
    conteudo = "A eficiência do sistema depende do equilíbrio entre crescimento, sobrevivência e custo."
    tags = "eficiencia,producao,custos"
    clienteId = 1
},
@{
    conteudo = "A colheita deve ser planejada considerando o mercado e o peso ideal de comercialização."
    tags = "colheita,mercado,peso ideal"
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