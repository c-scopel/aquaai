$baseUrl = "http://localhost:5205/conhecimento"

$dados = @(

@{
    conteudo = "A análise de cenários produtivos permite avaliar diferentes estratégias de cultivo antes da execução."
    tags = "cenarios,planejamento,estrategia"
    clienteId = 1
},
@{
    conteudo = "A produção total depende da densidade de estocagem, sobrevivência e peso final dos organismos."
    tags = "producao,densidade,sobrevivencia,peso final"
    clienteId = 1
},
@{
    conteudo = "O faturamento é calculado com base na produção total e no preço de venda por quilo."
    tags = "faturamento,preco venda,producao"
    clienteId = 1
},
@{
    conteudo = "A variação no preço de venda impacta diretamente a rentabilidade do sistema."
    tags = "preco venda,rentabilidade,mercado"
    clienteId = 1
},
@{
    conteudo = "A sobrevivência é um dos principais fatores que determinam o sucesso econômico da produção."
    tags = "sobrevivencia,resultado economico"
    clienteId = 1
},
@{
    conteudo = "A conversão alimentar influencia diretamente o custo de produção."
    tags = "conversao alimentar,custo producao"
    clienteId = 1
},
@{
    conteudo = "O custo com ração representa a maior parcela dos custos operacionais em sistemas intensivos."
    tags = "racao,custo alto,operacional"
    clienteId = 1
},
@{
    conteudo = "Custos com energia elétrica são relevantes em sistemas com alta demanda de aeração."
    tags = "energia,aeracao,custos"
    clienteId = 1
},
@{
    conteudo = "A densidade elevada pode aumentar a produção, mas também eleva o risco operacional."
    tags = "densidade,risco,producao"
    clienteId = 1
},
@{
    conteudo = "A margem de lucro é obtida pela diferença entre faturamento e custos totais."
    tags = "lucro,margem,custos"
    clienteId = 1
},
@{
    conteudo = "A análise de múltiplos cenários permite identificar o melhor equilíbrio entre risco e retorno."
    tags = "cenarios,risco,retorno"
    clienteId = 1
},
@{
    conteudo = "Cenários com baixa sobrevivência devem ser considerados para avaliação de risco."
    tags = "risco,sobrevivencia baixa"
    clienteId = 1
},
@{
    conteudo = "Cenários com alta eficiência produtiva apresentam melhor rentabilidade."
    tags = "eficiencia,rentabilidade"
    clienteId = 1
},
@{
    conteudo = "O custo por quilo produzido é um indicador essencial para análise econômica."
    tags = "custo por kg,indicador"
    clienteId = 1
},
@{
    conteudo = "A previsibilidade de produção melhora com o uso de dados históricos e simulações."
    tags = "previsibilidade,simulacao"
    clienteId = 1
},
@{
    conteudo = "A análise de sensibilidade permite entender o impacto de variáveis como preço e sobrevivência."
    tags = "analise sensibilidade,variaveis"
    clienteId = 1
},
@{
    conteudo = "O planejamento financeiro deve considerar cenários otimistas, realistas e pessimistas."
    tags = "planejamento financeiro,cenarios"
    clienteId = 1
},
@{
    conteudo = "A eficiência do sistema depende do controle simultâneo de custos e desempenho produtivo."
    tags = "eficiencia,custos,producao"
    clienteId = 1
},
@{
    conteudo = "A tomada de decisão deve ser baseada em dados e não apenas em estimativas."
    tags = "decisao,dados"
    clienteId = 1
},
@{
    conteudo = "A comparação entre cenários permite otimizar a estratégia de cultivo."
    tags = "comparacao,otimizacao"
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