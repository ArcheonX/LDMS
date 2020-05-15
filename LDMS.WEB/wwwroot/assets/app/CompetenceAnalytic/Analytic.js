var topics = [];
var employees = [];
var scores = []; 
var chart = null;
(function ($) {
    "use strict";
    $(document).ready(function () {
        var analytic_id = $("#analytic_id").val();
        $('#btnBack').click(function () {
            window.location.href = "/Competence/Index";
        });
        $('#btnEditCompetence').click(function () {
            window.location.href = "/Competence/Edit/" + $("#analytic_id").val();
        });
        $('#btnSaveCompetence').click(function () {
            SaveCompetenceScore();
        });  
        $('#btnImportExcel').click(function () {
            ExportCompetenceScore(analytic_id);
        });   
        LoadCompetence(parseInt(analytic_id)); 
    })
})(jQuery);

function validate(control, evt) {
    var theEvent = evt || window.event; 
    // Handle paste
    if (theEvent.type === 'paste') {
        key = event.clipboardData.getData('text/plain');
    } else {
        // Handle key press
        var key = theEvent.keyCode || theEvent.which;
        key = String.fromCharCode(key);
    }
    var regex = /[0-5]|\./;
    if (!regex.test(key)) {
        theEvent.returnValue = false;
        if (theEvent.preventDefault) theEvent.preventDefault();
    } 
}

function RenderChart() {
    var AnalyticCategories = [];
    var Analyticseries = []; 
    //if (topics.length > 6 || employees.length > 6) {
    //    return;
    //}
    var topicScore = []; 
    topics.forEach(tp => { 
        AnalyticCategories.push(tp.Topic);
        var score = scores.where((sc) => {
            return sc.ID_CompetenceKnowledgeTopic == tp.TopicId;
        });
        score.forEach(scr => {
            topicScore.push({
                EmployeeId: scr.ID_CompetenceEmployee,
                Score: scr.Score
            });
        });
    });
    employees.forEach(emp => {
        var AnalyticScores = [];
        var eScores = topicScore.where((sc) => {
            return sc.EmployeeId == emp.EmployeeId;
        });
        eScores.forEach(scr => {
            AnalyticScores.push(scr.Score);
        });
        Analyticseries.push(
            {
                name: emp.EmployeeName,
                data: AnalyticScores,
                pointPlacement: 'on'
            });
    }); 
    chart =  Highcharts.chart('container', {
        chart: {
            polar: true,
            type: 'line' 
        },
        exporting: {
            enabled: false,
            buttons: {
                enabled: false
            }
        },
        title: {
            text: $("#txtCompetenceName").val() + ' Competence Analytic', 
            x: -80
        },
        pane: {
            size: '80%'
        },
        xAxis: {
            categories: AnalyticCategories,
            tickmarkPlacement: 'on',
            lineWidth: 0,
            labels: {
                align: 'center', 
            }
        },
        yAxis: {
            gridLineInterpolation: 'polygon',
            lineWidth: 0,
            min: 0,
            tickPositions: [0, 1, 2, 3, 4, 5]
        },
        tooltip: {
            shared: true,
            headerFormat: '<span style="font-size: 12px">{point.key}:</span>&nbsp;&nbsp;<b>{point.y:,.2f}</b>',
            pointFormat: '',
            useHTML: true
        },
        legend: {
            layout: 'vertical',
            align: 'right',
            verticalAlign: 'middle',
            borderWidth: 0
        },
        credits: {
            enabled: false,
        },
        series: Analyticseries 
    });
}

function LoadCompetence(analytic_id) { 
    MessageController.BlockUI({ boxed: true, textOnly: true, target: '#pn-Analytic' });
    topics = [];
    employees = [];
    scores = [];
    expectatoins = [];
    $.ajax({
        type: "GET",
        url: "/Competence/Competence",
        data: { "competenceId": analytic_id },
        success: function (response) {

            $("#txtCompetenceName").val(response.Data.CompetenceAnalyticName);
            $("#txtExpectatoin5").val("5 = " + response.Data.Criteria5);
            $("#txtExpectatoin4").val("4 = " + response.Data.Criteria4);
            $("#txtExpectatoin3").val("3 = " + response.Data.Criteria3);
            $("#txtExpectatoin2").val("2 = " + response.Data.Criteria2);
            $("#txtExpectatoin1").val("1 = " + response.Data.Criteria1);
            $("#lastUpdate").text(moment(response.Data.LastUpdate).format("DD/MM/YYYY HH:mm:ss"));
            $("#selectLevel").val(response.Data.ID_JobGrade).trigger('change');             
            $.each(response.Data.Topics, function () {
                topics.push({
                    TopicId: this.ID,
                    ID_CompetenceAnalytic: this.ID_CompetenceAnalytic,
                    Topic: this.KnowledgeTopicName,
                    IsSpecial: this.ID_Course > 0 ? false : true,
                    ID_Course: this.ID_Course,
                    Expectatoin: this.Expectatoin
                }); 
            });  
            $.each(response.Data.Employees, function () {
                employees.push({ 
                    EmployeeId: this.EmployeeID,
                    EmployeeName: this.LDMS_M_User.FullName, 
                }); 
            });
            scores.push({
                ID_CompetenceKnowledgeTopic:0,
                ID_CompetenceEmployee: "",
                Score:0,
            });
            scores.pop();
            $.each(response.Data.Scores, function () {
                scores.push({  
                    ID_CompetenceKnowledgeTopic: this.ID_CompetenceKnowledgeTopic,
                    ID_CompetenceEmployee: this.ID_CompetenceEmployee,
                    Score: this.Score,
                    MinScore: this.MinScore,
                    MaxScore: this.MaxScore
                }); 
            }); 
            BuildTable(); 
            MessageController.UnblockUI('#pn-Analytic');
            $("input[type=number]").focus(function () {
                $(this).select();
            });
            $("input[type=number]").change(function () {
                var max = parseInt($(this).attr('max'));
                var min = parseInt($(this).attr('min'));
                if ($(this).val() > max) {
                    $(this).val(max);
                }
                else if ($(this).val() < min) {
                    $(this).val(min);
                }
            });      
            RenderChart(); 
        },
        failure: function (response) {
            MessageController.UnblockUI('#pn-Analytic');
            if (JSON.parse(response.responseText).Errors.length > 0) {
                MessageController.Error(JSON.parse(response.responseText).Errors[0].replace("Message:", ""), "Error");
            } else {
                MessageController.Error(response.responseText, "Error");
            }
        },
        error: function (response) {
            MessageController.UnblockUI('#pn-Analytic');
            if (JSON.parse(response.responseText).Errors.length > 0) {
                MessageController.Error(JSON.parse(response.responseText).Errors[0].replace("Message:", ""), "Error");
            } else {
                MessageController.Error(response.responseText, "Error");
            }
        }
    }); 
}

function BuildTable() {

    var table = document.getElementById('tableAnalytic');
    while (table.hasChildNodes()) {
        table.removeChild(table.firstChild);
    }

    var thead = document.createElement('thead');
    var tr = document.createElement('tr');
    var topic = document.createElement('th');
    topic.className = "thTopic";
    topic.style = "vertical-align: middle;";
    topic.appendChild(document.createTextNode('Knowledge Topic'));

    var expec = document.createElement('th');
    expec.className = "thExpectatoin";
    expec.style = "vertical-align: middle;";
    $("<div  class='rotated'>Expectatoin</div>").appendTo(expec); 

    tr.appendChild(topic);
    tr.appendChild(expec);
    employees.forEach(item => {
        var thCol = document.createElement('th');
        thCol.className = "verticalTableHeader";
        thCol.style = "vertical-align: middle;";
        $("<div  class='rotated'>" + item.EmployeeName + "</div>").appendTo(thCol);  
        tr.appendChild(thCol);
    });
    thead.appendChild(tr);
    var tbody = document.createElement('tbody');
    topics.forEach(item => {

        var tr_tpoic = document.createElement('tr');
        var tdtpoic = document.createElement('td');

        tdtpoic.className = "tdTopic";
        tdtpoic.appendChild(document.createTextNode(item.Topic));
        tr_tpoic.appendChild(tdtpoic);


        var tdExpectatoin = document.createElement('td');
        tdExpectatoin.className = "tdScore";
        tdExpectatoin.style = "width:50px;";
        tr_tpoic.appendChild(tdExpectatoin); 
        $("<input type='number' disabled id='txtScore_topic_" + item.TopicId + "_Expectatoin' style='width:100%;text-align:center' onkeypress='validate(this,event)' class='quantity'   min='0'  max='5' value='" + item.Expectatoin + "'  />").appendTo(tdExpectatoin);        
        employees.forEach(emp => {
            var score_value = 0;
            var minScore = 0;
            var maxScore = 0;
            var any_score = scores.where((sc) => {
                return sc.ID_CompetenceKnowledgeTopic == item.TopicId && sc.ID_CompetenceEmployee == emp.EmployeeId;
            });
            if (any_score && any_score.length > 0) {
                score_value = any_score[0].Score;
                minScore = any_score[0].MinScore;
                maxScore = any_score[0].MaxScore;
            }
            var td = document.createElement('td');
            td.className = "tdScore";
            td.style = "width:50px;";
            tr_tpoic.appendChild(td);
            $("<input type='number' id='txtScore_topic_" + item.TopicId + "_Employee_" + emp.EmployeeId + "' style='width:100%;text-align:center' onkeypress='validate(this,event)' class='quantity'   min='" + minScore + "'  max='" + maxScore +"' value='" + score_value + "' />").appendTo(td);
            
        });
        tbody.appendChild(tr_tpoic);
    });
    table.appendChild(thead);
    table.appendChild(tbody);
}

function SaveCompetenceScore() {
    var analytic_id = $("#analytic_id").val();
    var analyticexpectatoins = [];
    var analyticscores = []; 
    topics.forEach(item => {
        var score = $("#txtScore_topic_" + item.TopicId + '_Expectatoin').val();
        if (score == null || score == undefined) {
            score = "0";
        }
        analyticexpectatoins.push(
            {
                "ID_CompetenceAnalytic": String(analytic_id),
                "ID_CompetenceKnowledgeTopic": String(item.TopicId),
                "Score": String(score)
            });
        employees.forEach(emp => {
            var empscore = $("#txtScore_topic_" + item.TopicId + '_Employee_' + emp.EmployeeId).val();
            if (empscore == null || empscore == undefined) {
                empscore = "0";
            }
            analyticscores.push(
                {
                    "ID_CompetenceKnowledgeTopic": String(item.TopicId) ,
                    "ID_CompetenceEmployee": String(emp.EmployeeId),
                    "Score": String(empscore)
                });
        });
    });

    $.ajax({
        type: "POST",
        url: "/Competence/UpdateCompetenceScore",
        data:
        {
            "competenceId": analytic_id,
            "scores":analyticscores,
            "expectatoins": analyticexpectatoins
        },
        success: function (response) {
            MessageController.Success("Update Competence Score Completed.", "Completed");
            LoadCompetence(analytic_id);
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

function ExportCompetenceScore(analytic_id) {
    MessageController.BlockUI({ boxed: true, target: '#pn-Analytic' });
    debugger;
    var svgString = chart.getSVG().replace(/</g, '\n&lt;').replace(/>/g, '&gt;'); 
    //const blob = new Blob([svgString], { type: 'image/svg+xml' });
    //const url = URL.createObjectURL(blob);
    //const image = document.createElement('img');

    //let parser = new DOMParser();
    //let svgElem = parser.parseFromString(svgString, "image/svg+xml").documentElement;
    //let base64 = svgElem.toDataURL();
   // var formData = new FormData();
    //formData.append('fileImage', blob);
    //formData.append('competenceId', analytic_id);
    $.ajax({
        type: "POST",
        url: '/Competence/Export', 
        data: {
            'competenceId': analytic_id,
            'chartImage': JSON.stringify(svgString)
        },
        success: function (response) {
            var data = response.Data;
            if (data != "") {
                window.location.href = "/Competence/Download/?fileName=" + data;
            }
            MessageController.UnblockUI('#pn-Analytic');
        },
        failure: function (response) {
            MessageController.UnblockUI('#pn-Analytic');
            if (JSON.parse(response.responseText).Errors.length > 0) {
                MessageController.Error(JSON.parse(response.responseText).Errors[0].replace("Message:", ""), "Error");
            } else {
                MessageController.Error(response.responseText, "Error");
            }
        },
        error: function (response) {
            MessageController.UnblockUI('#pn-Analytic');
            if (JSON.parse(response.responseText).Errors.length > 0) {
                MessageController.Error(JSON.parse(response.responseText).Errors[0].replace("Message:", ""), "Error");
            } else {
                MessageController.Error(response.responseText, "Error");
            }
        }
    });
    //var chartSelector = jThis.data("chartCompetence");
    //var chart = $(chartSelector).highcharts(); 
    //if (Highcharts.exporting.supports("image/jpeg")) {
    //    chart.exportChartLocal({
    //        type: "image/jpeg"
    //    });
    //}
}