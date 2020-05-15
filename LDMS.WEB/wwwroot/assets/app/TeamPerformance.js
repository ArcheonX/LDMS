(function ($) {
    "use strict";
    $(document).ready(function () {   
        $('select').select2({
            closeOnSelect: true,
            theme: "bootstrap",
            allowClear: true,
            placeholder: 'All' 
        }); 
        LoadPlant();
        $('select[name="selectPlant"]').on('change', function () {
            var plantId = $(this).val();
            LoadCenter(plantId);
        });
        $('select[name="selectCenter"]').on('change', function () {
            var centerId = $(this).val();
            LoadDivision(centerId);
        });
        $('select[name="selectDivision"]').on('change', function () {
            var divisionId = $(this).val();
            LoadDepartment(divisionId);
        });
        $('select[name="selectDepartment"]').on('change', function () {
            var departmentId = $(this).val(); 
            LoadSection(departmentId);
        });

        var year = new Date().getFullYear();
        $("#FY3").val(year);
        $("#FY2").val(year - 1);
        $("#FY1").val(year - 2);
        $("#lblFY3").text("FY" + year);
        $("#lblFY2").text("FY" + (year - 1));
        $("#lblFY1").text("FY" + (year - 2));

        $('#btnQueryProgress').click(function () {
            SearchTeamPerformance();
        });
        SearchTeamPerformance(); 
    })
})(jQuery);

function LoadPlant() {
    $.ajax({
        type: "GET",
        url: "/Master/GetAllPlants",
        success: function (response) {
            var options = $('#selectPlant');
            options.append($("<option />").val(null).text("---All---"));
            $.each(response.Data, function () {
                options.append($("<option />").val(this.ID_Plant).text('(' + this.PlantID + ') ' + this.PlantName_EN));
            });

            var plantId = CookiesController.getCookie("PLANTID");
            var isfound = response.Data.any((item) => {
                return item.ID_Plant == plantId;
            });
            if (isfound) {
                $('select[name="selectPlant"]').val(plantId).trigger('change');
                $('select[name="selectPlant"]').attr('disabled', 'disabled');
            } else {
                $('select[name="selectPlant"]').val(null).trigger('change');
            }
        },
        failure: function (response) {
            if (JSON.parse(response.responseText).Errors.length > 0) {
                MessageController.Error(JSON.parse(response.responseText).Errors[0].replace("Message:", ""), "Error");
            } else {
                MessageController.Error(response.responseText, "Error");
            }
        },
        error: function (response) {
            if (JSON.parse(response.responseText).Errors.length > 0) {
                MessageController.Error(JSON.parse(response.responseText).Errors[0].replace("Message:", ""), "Error");
            } else {
                MessageController.Error(response.responseText, "Error");
            }
        }
    });
}

function LoadCenter(plantId) {
    var options = $('#selectCenter');
    options.empty();
    $.ajax({
        type: "GET",
        url: "/Master/GetAllCentersByPlant",
        data: { 'plantId': plantId },
        success: function (response) {
            options.append($("<option />").val(null).text("--All--"));
            $.each(response.Data, function () {
                options.append($("<option />").val(this.ID_Center).text('(' + this.CenterID + ') ' + this.CenterName_EN));
            }); 

            var centerId = CookiesController.getCookie("CENTERID");
            var isfound = response.Data.any((item) => {
                return item.ID_Center == centerId;
            }); 
            if (isfound) {
                $('select[name="selectCenter"]').val(centerId).trigger('change');
                $('select[name="selectCenter"]').attr('disabled', 'disabled');
            } else {
                $('select[name="selectCenter"]').val(null).trigger('change');
            } 
        },
        failure: function (response) {
            if (JSON.parse(response.responseText).Errors.length > 0) {
                MessageController.Error(JSON.parse(response.responseText).Errors[0].replace("Message:", ""), "Error");
            } else {
                MessageController.Error(response.responseText, "Error");
            }
        },
        error: function (response) {
            if (JSON.parse(response.responseText).Errors.length > 0) {
                MessageController.Error(JSON.parse(response.responseText).Errors[0].replace("Message:", ""), "Error");
            } else {
                MessageController.Error(response.responseText, "Error");
            }
        }
    });
}

function LoadDivision(centerId) {
    var options = $('#selectDivision');
    options.empty();
    $.ajax({
        type: "GET",
        url: "/Master/GetAllDivisionsByCenter",
        data: {
            'centerId': centerId
        },
        success: function (response) {
            options.append($("<option />").val(null).text("--All--"));
            $.each(response.Data, function () {
                options.append($("<option />").val(this.ID_Division).text('(' + this.DivisionID + ') ' + this.DivisionName_EN));
            }); 
            var divisionId = CookiesController.getCookie("DIVISIONID");
            var isfound = response.Data.any((item) => {
                return item.ID_Division == divisionId;
            });  
            if (isfound) {
                $('select[name="selectDivision"]').val(divisionId).trigger('change');
                $('select[name="selectDivision"]').attr('disabled', 'disabled');
            } else {
                $('select[name="selectDivision"]').val(null).trigger('change');
            }
        },
        failure: function (response) {
            if (JSON.parse(response.responseText).Errors.length > 0) {
                MessageController.Error(JSON.parse(response.responseText).Errors[0].replace("Message:", ""), "Error");
            } else {
                MessageController.Error(response.responseText, "Error");
            }
        },
        error: function (response) {
            if (JSON.parse(response.responseText).Errors.length > 0) {
                MessageController.Error(JSON.parse(response.responseText).Errors[0].replace("Message:", ""), "Error");
            } else {
                MessageController.Error(response.responseText, "Error");
            }
        }
    });
}

function LoadDepartment(divisionId) {
    var options = $('#selectDepartment');
    options.empty();
    $.ajax({
        type: "GET",
        url: "/Master/GetAllDepartmentsByDivision",
        data: {
            'divisionId': divisionId
        },
        success: function (response) {

            options.append($("<option />").val(null).text("--All--"));
            $.each(response.Data, function () {
                options.append($("<option />").val(this.ID_Department).text('(' + this.DepartmentID + ') ' + this.DepartmentName_EN));
            });

            var departmentId = CookiesController.getCookie("DEPARTMENTID");
            var isfound = response.Data.any((item) => {
                return item.ID_Department == departmentId;
            }); 
            if (isfound) {
                $('select[name="selectDepartment"]').val(departmentId).trigger('change');
                $('select[name="selectDepartment"]').attr('disabled', 'disabled');
            } else {
                $('select[name="selectDepartment"]').val(null).trigger('change');
            }
        },
        failure: function (response) {
            if (JSON.parse(response.responseText).Errors.length > 0) {
                MessageController.Error(JSON.parse(response.responseText).Errors[0].replace("Message:", ""), "Error");
            } else {
                MessageController.Error(response.responseText, "Error");
            }
        },
        error: function (response) {
            if (JSON.parse(response.responseText).Errors.length > 0) {
                MessageController.Error(JSON.parse(response.responseText).Errors[0].replace("Message:", ""), "Error");
            } else {
                MessageController.Error(response.responseText, "Error");
            }
        }
    });
}

function LoadSection(departmentId) {
    var options = $('#selectSection');
    if (departmentId == null || departmentId == undefined) {
        $('select[name="selectSection"]').attr('disabled', 'disabled');
    } else{
        $('select[name="selectSection"]').removeAttr('disabled', 'disabled');
    }
    options.empty();
    $.ajax({
        type: "GET",
        url: '/Master/GetAllSectionsByDepartment',
        data: {
            'departmentId': departmentId
        },
        success: function (response) {
            options.append($("<option />").val(null).text("Please select"));
            $.each(response.Data, function () {
                options.append($("<option />").val(this.ID_Section).text('(' + this.SectionID + ') ' + this.SectionName_EN));
            });
        
            $('select[name="selectSection"]').val(null).trigger('change');
        },
        failure: function (response) {
            if (JSON.parse(response.responseText).Errors.length > 0) {
                MessageController.Error(JSON.parse(response.responseText).Errors[0].replace("Message:", ""), "Error");
            } else {
                MessageController.Error(response.responseText, "Error");
            }
        },
        error: function (response) {
            if (JSON.parse(response.responseText).Errors.length > 0) {
                MessageController.Error(JSON.parse(response.responseText).Errors[0].replace("Message:", ""), "Error");
            } else {
                MessageController.Error(response.responseText, "Error");
            }
        }
    });
}

function RenderChartProgress(data) {
    var overdue = 0;
    var notstart = 0;
    var onprogress = 0;
    var completed = 0;
    var completedPct = 0;
    if (data != null && data.Progress != null) {
        overdue = data.Progress.Overdue;
        notstart = data.Progress.NotStart;
        onprogress = data.Progress.OnProgress;
        completed = data.Progress.Completed;
        completedPct = Math.round((completed / data.List.length) * 100);
    }

    Highcharts.chart('learningProgressContainer', {
        credits: {
            enabled: false,
        },
        chart: {
            plotBackgroundColor: null,
            plotBorderWidth: null,
            plotShadow: false,
            type: 'pie'
        },
        exporting: {
            enabled: false,
            buttons: {
                enabled: false
            }
        },
        title: {
            text: completedPct + '%',
            align: 'center',
            verticalAlign: 'middle',
            y: 60
        },
        tooltip: {
            pointFormat: '{series.name}: <b>{point.percentage:.1f}%</b>'
        },
        accessibility: {
            point: {
                valueSuffix: '%'
            }
        },
        plotOptions: {
            pie: {
                shadow: false,
                dataLabels: {
                    enabled: true,
                    distance: -50,
                    style: {
                        color: 'black',
                        textShadow: false,
                        textOutline: false
                    }
                },
                showInLegend: true,
                startAngle: -90,
                endAngle: 90,
                center: ['50%', '75%'],
                borderWidth: 0,
                size: '100%'
            }
        },
        colors: ['#E50000', '#EEEEEE', '#F0F20D', '#8BBC21'],
        series: [{
            type: 'pie',
            name: 'Overall My Learning Progress',
            dataLabels: {
                color: 'black',
                shadow: false,
                borderWidth: 0,
                distance: -30,
                formatter: function () {
                    if (this.percentage != 0) return Math.round(this.percentage);

                }
            },
            innerSize: '60%',
            data: [
                ['Over Due', overdue],
                ['Not Start', notstart],
                ['On Progress', onprogress],
                ['Completed', completed]
            ]
        }]
    }); 
}

function RenderChartBySection(data) {
    var cates = [];
    var overdue = [];
    var notstart = [];
    var onprogress = [];
    var completed = [];

    if (data != null && data.Sections.length > 0) {
        $.each(data.Sections, function () {
            cates.push(this.SectionName);
            overdue.push(this.Overdue);
            notstart.push(this.NotStart);
            onprogress.push(this.OnProgress);
            completed.push(this.Completed);
        });
    }
    Highcharts.chart('BySectionContainer', {
        credits: {
            enabled: false,
        },
        chart: {
            type: 'bar'
        },
        exporting: {
            enabled: false,
            buttons: {
                enabled: false
            }
        },
        title: {
            text: ''
        },
        xAxis: {
            categories: cates
        },
        yAxis: {
            title: {
                text: ''
            },
            labels: {
                enabled: false
            }
        },
        //legend: {
        //    reversed: true
        //},
        plotOptions: {
            bar: {
                stacking: 'percent'
            }
        },
        colors: ['#E50000', '#EEEEEE', '#F0F20D', '#8BBC21'],
        series:
            [
                {
                    name: 'Over Due',
                    dataLabels: {
                        enabled: true,
                        style: {
                            textShadow: false,
                            textOutline: false
                        },
                        color: 'black',
                        formatter: function () {
                            if (this.percentage != 0) return Math.round(this.percentage);

                        }
                    },
                    data: overdue
                },
                {
                    name: 'Not Start',
                    dataLabels: {
                        enabled: true,
                        style: {
                            textShadow: false,
                            textOutline: false
                        },
                        color: 'black',
                        formatter: function () {
                            if (this.percentage != 0) return Math.round(this.percentage);

                        }
                    },
                    data: notstart
                },
                {
                    name: 'On Progress',
                    dataLabels: {
                        enabled: true,
                        style: {
                            textShadow: false,
                            textOutline: false
                        },
                        color: 'black',
                        formatter: function () {
                            if (this.percentage != 0) return Math.round(this.percentage);

                        }
                    },
                    data: onprogress
                }, {
                    name: 'Completed',
                    dataLabels: {
                        enabled: true,
                        style: {
                            textShadow: false,
                            textOutline: false
                        },
                        color: 'black',
                        formatter: function () {
                            if (this.percentage != 0) return Math.round(this.percentage);

                        }
                    },
                    data: completed
                }]
    }); 
}

function RenderChartByPlatform(data) {
    var cates = [];
    var overdue = [];
    var notstart = [];
    var onprogress = [];
    var completed = [];

    if (data != null && data.PlatForms.length > 0) {
        $.each(data.PlatForms, function () {
            cates.push(this.PlatformName);
            overdue.push(this.Overdue);
            notstart.push(this.NotStart);
            onprogress.push(this.OnProgress);
            completed.push(this.Completed);
        });
    }
    Highcharts.chart('ByPlatformContainer', {
        credits: {
            enabled: false,
        },
        chart: {
            type: 'bar'
        },
        exporting: {
            enabled: false,
            buttons: {
                enabled: false
            }
        },
        title: {
            text: ''
        },
        xAxis: {
            categories: cates
        },
        yAxis: {
            title: {
                text: ''
            },
            labels: {
                enabled: false
            }
        },
        //legend: {
        //    reversed: true
        //},
        plotOptions: {
            bar: {
                stacking: 'percent'
            }
        },
        colors: ['#E50000', '#EEEEEE', '#F0F20D', '#8BBC21'],
        series:
            [
                {
                    name: 'Over Due',
                    dataLabels: {
                        enabled: true,
                        style: {
                            textShadow: false,
                            textOutline: false
                        },
                        color: 'black',
                        formatter: function () {
                            if (this.percentage != 0) return Math.round(this.percentage);

                        }
                    },
                    data: overdue
                },
                {
                    name: 'Not Start',
                    dataLabels: {
                        enabled: true,
                        style: {
                            textShadow: false,
                            textOutline: false
                        },
                        color: 'black',
                        formatter: function () {
                            if (this.percentage != 0) return Math.round(this.percentage);

                        }
                    },
                    data: notstart
                },
                {
                    name: 'On Progress',
                    dataLabels: {
                        enabled: true,
                        style: {
                            textShadow: false,
                            textOutline: false
                        },
                        color: 'black',
                        formatter: function () {
                            if (this.percentage != 0) return Math.round(this.percentage);

                        }
                    },
                    data: onprogress
                }, {
                    name: 'Completed',
                    dataLabels: {
                        enabled: true,
                        style: {
                            textShadow: false,
                            textOutline: false
                        },
                        color: 'black',
                        formatter: function () {
                            if (this.percentage != 0) return Math.round(this.percentage);

                        }
                    },
                    data: completed
                }]
    });
}

function RenderChartByJobLevel(data) {

    var cates = [];
    var overdue = [];
    var notstart = [];
    var onprogress = [];
    var completed = [];

    if (data != null && data.Levels.length > 0) {
        $.each(data.Levels, function () {
            cates.push(this.LevelName);
            overdue.push(this.Overdue);
            notstart.push(this.NotStart);
            onprogress.push(this.OnProgress);
            completed.push(this.Completed);
        });
    } 
    Highcharts.chart('ByJobLevelContainer', {
        chart: {
            type: 'column'
        },
        credits: {
            enabled: false,
        },
        exporting: {
            enabled: false,
            buttons: {
                enabled: false
            }
        },
        title: {
            text: ''
        },
        xAxis: {
            categories: cates
        },
        yAxis: {
            min: 0,
            title: {
                text: ''
            },
            stackLabels: {
                enabled: false,
                style: {
                    fontWeight: 'bold',
                    color:'black'
                }
            }
        },
        legend: {
            align: 'center', 
            verticalAlign: 'bottom',
            y: 25,
            floating: true,  
            shadow: false
        },
        tooltip: {
            headerFormat: '<b>{point.x}</b><br/>',
            pointFormat: '{series.name}: {point.y}<br/>Total: {point.stackTotal}'
        },
        plotOptions: {
            column: {
                stacking: 'percent' 
            }
        },
        series: [{
            name: 'Not Start',
            color: '#EEEEEE',
            data: notstart,
            dataLabels: {
                enabled: true,
                style: {
                    textShadow: false,
                    textOutline: false
                },
                color: 'black',
                formatter: function () {
                    if (this.percentage != 0) return Math.round(this.percentage);

                }
            }
        }, {
                name: 'On Progress',
                color: '#f2f411',
                data: onprogress,
                dataLabels: {
                    enabled: true,
                    style: {
                        textShadow: false,
                        textOutline: false
                    },
                    color: 'black',
                    formatter: function () {
                        if (this.percentage != 0) return Math.round(this.percentage);

                    }
                }
            }, {
            name: 'Completed',
                color: '#8BBC21',
                data: completed,
                dataLabels: {
                    enabled: true,
                    style: {
                        textShadow: false,
                        textOutline: false
                    },
                    color: 'black',
                    formatter: function () {
                        if (this.percentage != 0) return Math.round(this.percentage);

                    }
                }
        }
            , {
                name: 'Over Due',
                color: '#E50000',
                data: overdue,
                dataLabels: {
                    enabled: true,
                    style: {
                        textShadow: false,
                        textOutline: false
                    },
                    color: 'black',
                    formatter: function () {
                        if (this.percentage != 0) return Math.round(this.percentage);

                    }
                }
            }]
    }); 
}

function RenderChartCostSpending(data) {

    var cates = [];
    var invest = [];
    var qualifiled = [];
    var lost = [];
    
    if (data != null && data.Costs.length > 0) {
        $.each(data.Costs, function () {
            cates.push(this.MonthName);
            invest.push(roundToTwo(this.Invest / 1000));
            qualifiled.push(roundToTwo(this.Qualifiled / 1000));
            lost.push(roundToTwo(this.Lost / 1000));
        });
    } 
    Highcharts.chart('LearningCostSpendingContainer', {
        chart: {
            type: 'line'
        },
        credits: {
            enabled: false,
        },
        exporting: {
            enabled: false,
            buttons: {
                enabled: false
            }
        },
        title: {
            text: ''
        },
        subtitle: {
            text: ''
        },
        xAxis: {
            categories: cates
        },
        yAxis: {
            title: {
                text: ''
            }
        },
        plotOptions: {
            line: {
                dataLabels: {
                    enabled: true
                },
                enableMouseTracking: false
            }
        }, 
        series: [{
            name: 'Invest(K Bath)',
            color: '#171BF4',
            data: invest
        }, {
                name: 'Qualifiled(K Bath)',
            color: '#78ff48',
            data: qualifiled
        }, {
                name: 'Lost(K Bath)',
            color: '#E50000',
            data: lost
        }]
    });
}

function SearchTeamPerformance() {
    MessageController.BlockUI({ boxed: true, textOnly: true, target: '#pn-result' }); 
    var fyear = $("input[type=radio][name=FixicalYear]:checked").val();//$("input[type='radio']:checked").val();
    var quater = [];
    $('input[type=checkbox][name=Quater]:checked').each(function () {
        quater.push(this.value);
    });
    $.ajax({
        type: "GET",
        url: "/Course/TeamPerformance",
        data:
        { 
            "ficialYear": fyear,
            "quater": quater.join(","),
            "plantId": $('select[name="selectPlant"]').val(),
            "centerId": $('select[name="selectCenter"]').val(),
            "divisionId": $('select[name="selectDivision"]').val(),
            "departmentId": $('select[name="selectDepartment"]').val(),
            "sectionId": $('select[name="selectSection"]').val()
        },
        success: function (response) {
            RenderChartProgress(response.Data);
            RenderChartBySection(response.Data);
            RenderChartByPlatform(response.Data);
            RenderChartByJobLevel(response.Data);
            RenderChartCostSpending(response.Data);

            $("#txtTotalCourse").text(response.Data.TotalCourse);
            $("#txtTarget").text(response.Data.TotalTarget);
            MessageController.UnblockUI('#pn-result');
        },
        failure: function (response) {
            MessageController.UnblockUI('#pn-result');
            if (JSON.parse(response.responseText).Errors.length > 0) {
                MessageController.Error(JSON.parse(response.responseText).Errors[0].replace("Message:", ""), "Error");
            } else {
                MessageController.Error(response.responseText, "Error");
            }
        },
        error: function (response) {
            MessageController.UnblockUI('#pn-result');
            if (JSON.parse(response.responseText).Errors.length > 0) {
                MessageController.Error(JSON.parse(response.responseText).Errors[0].replace("Message:", ""), "Error");
            } else {
                MessageController.Error(response.responseText, "Error");
            }
        }
    });
}

function roundToTwo(num) {
    return +(Math.round(num + "e+2") + "e-2");
}

