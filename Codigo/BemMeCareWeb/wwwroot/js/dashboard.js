(function () {
    const selectTipo = document.getElementById('selectTipoFiltro');
    const selectItem = document.getElementById('selectItemFiltro');

    if (selectTipo && selectItem) {
        function atualizarItens() {
            const tipoAtual = selectTipo.value;
            let primeiroVisivel = null;
            let selecionadoVisivel = false;

            selectItem.querySelectorAll('option').forEach(function (opt) {
                const corresponde = opt.dataset.tipo === tipoAtual;
                opt.hidden = !corresponde;
                opt.disabled = !corresponde;

                if (corresponde) {
                    if (!primeiroVisivel) primeiroVisivel = opt;
                    if (opt.selected) selecionadoVisivel = true;
                } else {
                    opt.selected = false;
                }
            });

            if (!selecionadoVisivel && primeiroVisivel) {
                primeiroVisivel.selected = true;
            }
        }

        selectTipo.addEventListener('change', atualizarItens);
        atualizarItens();
    }

    const canvasPie = document.getElementById('graficoServicosPie');
    if (canvasPie && canvasPie.dataset.labels && canvasPie.dataset.valores) {
        const labels = JSON.parse(canvasPie.dataset.labels);
        const percentuais = JSON.parse(canvasPie.dataset.valores);

        new Chart(canvasPie, {
            type: 'pie',
            data: {
                labels: labels,
                datasets: [{
                    data: percentuais,
                    backgroundColor: [
                        '#36A2EB',
                        '#FF6384',
                        '#4BC0C0',
                        '#FFCE56',
                        '#9966FF',
                        '#FF9F40'
                    ]
                }]
            },
            options: {
                responsive: true,
                plugins: {
                    legend: {
                        position: 'bottom'
                    },
                    tooltip: {
                        callbacks: {
                            label: function (context) {
                                return context.label + ': ' + context.parsed + '%';
                            }
                        }
                    }
                }
            }
        });
    }

    const canvas12Meses = document.getElementById('graficoUltimos12Meses');
    if (canvas12Meses && canvas12Meses.dataset.labels && canvas12Meses.dataset.valores) {
        const labels12 = JSON.parse(canvas12Meses.dataset.labels);
        const valores12 = JSON.parse(canvas12Meses.dataset.valores);

        new Chart(canvas12Meses, {
            type: 'bar',
            data: {
                labels: labels12,
                datasets: [{
                    label: 'Faturamento (R$)',
                    data: valores12,
                    backgroundColor: '#8c3b4d'
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false }
                },
                scales: {
                    y: { beginAtZero: true }
                }
            }
        });
    }

    const canvasFiltrado = document.getElementById('graficoFaturamentoFiltrado');
    if (canvasFiltrado && canvasFiltrado.dataset.labels && canvasFiltrado.dataset.valores) {
        const labelsFiltro = JSON.parse(canvasFiltrado.dataset.labels);
        const valoresFiltro = JSON.parse(canvasFiltrado.dataset.valores);

        new Chart(canvasFiltrado, {
            type: 'bar',
            data: {
                labels: labelsFiltro,
                datasets: [{
                    label: 'Faturamento (R$)',
                    data: valoresFiltro,
                    backgroundColor: '#36A2EB',
                    maxBarThickness: 80
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false }
                },
                scales: {
                    y: { beginAtZero: true }
                }
            }
        });
    }
})();