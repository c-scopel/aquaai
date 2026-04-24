$baseUrl = "http://localhost:5205/conhecimento"

$dados = @(

@{
    conteudo = "A simulação de cenários permite comparar diferentes estratégias produtivas antes da tomada de decisão."
    tags = "simulacao cenarios,estrategia,planejamento"
    clienteId = 1
},
@{
    conteudo = "A variação de produtividade impacta diretamente o faturamento total do cultivo."
    tags = "produtividade,faturamento,producao"
    clienteId = 1
},
@{
    conteudo = "O custo operacional total deve considerar ração, energia, mão de obra e insumos."
    tags = "custo operacional,racao,energia,mao de obra"
    clienteId = 1
},
@{
    conteudo = "A rentabilidade depende do equilíbrio entre custos de produção e preço de venda."
    tags = "rentabilidade,lucro,preco venda,custos"
    clienteId = 1
},
@{
    conteudo = "A taxa de sobrevivência influencia diretamente o volume final comercializado."
    tags = "sobrevivencia,volume final,producao"
    clienteId = 1
},
@{
    conteudo = "A conversão alimentar determina a eficiência no uso da ração e impacta os custos."
    tags = "conversao alimentar,eficiencia,custo racao"
    clienteId = 1
},
@{
    conteudo = "O aumento da densidade pode elevar a produção, mas exige maior controle de manejo e qualidade de água."
    tags = "densidade,manejo,qualidade agua"
    clienteId = 1
},
@{
    conteudo = "Custos com energia elétrica são críticos em sistemas intensivos com aeração contínua."
    tags = "energia,aeracao,custo alto"
    clienteId = 1
},
@{
    conteudo = "A análise de risco avalia possíveis perdas relacionadas a falhas operacionais e variações de mercado."
    tags = "risco,perdas,mercado,operacao"
    clienteId = 1
},
@{
    conteudo = "Cenários conservadores ajudam a prever situações de baixa performance e reduzir prejuízos."
    tags = "cenario conservador,risco,prejuizo"
    clienteId = 1
},
@{
    conteudo = "Cenários agressivos buscam maximizar produção e lucro, assumindo maiores riscos operacionais."
    tags = "cenario agressivo,alto risco,alta producao"
    clienteId = 1
},
@{
    conteudo = "A margem operacional indica a eficiência financeira do sistema produtivo."
    tags = "margem operacional,eficiencia financeira"
    clienteId = 1
},
@{
    conteudo = "O custo por quilo produzido é um dos principais indicadores de desempenho econômico."
    tags = "custo por kg,indicador financeiro"
    clienteId = 1
},
@{
    conteudo = "A receita líquida é obtida após a dedução de todos os custos operacionais do faturamento bruto."
    tags = "receita liquida,faturamento,custos"
    clienteId = 1
},
@{
    conteudo = "A análise comparativa entre cenários permite identificar a melhor estratégia produtiva."
    tags = "comparacao cenarios,melhor estrategia"
    clienteId = 1
},
@{
    conteudo = "A previsibilidade financeira melhora com o uso de dados históricos e simulações."
    tags = "previsibilidade,dados historicos,simulacao"
    clienteId = 1
},
@{
    conteudo = "O controle de custos é essencial para manter a viabilidade econômica em sistemas intensivos."
    tags = "controle custos,viabilidade"
    clienteId = 1
},
@{
    conteudo = "A eficiência produtiva reduz o impacto de variações negativas no mercado."
    tags = "eficiencia,resiliencia,mercado"
    clienteId = 1
},
@{
    conteudo = "Decisões baseadas em dados aumentam a segurança e reduzem incertezas na produção."
    tags = "decisao baseada em dados,seguranca"
    clienteId = 1
},
@{
    conteudo = "A análise contínua de desempenho permite ajustes rápidos na estratégia de produção."
    tags = "monitoramento,ajuste estrategia"
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