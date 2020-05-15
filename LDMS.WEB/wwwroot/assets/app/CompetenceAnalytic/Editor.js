var topics = [];
var employees = [];
var tmpselection = [];
//var expectatoins = [];
(function ($) {
    "use strict";
    $(document).ready(function () {

        $("input[type='datetime']").datepicker();
        $('select').select2({
            allowClear: true,
            closeOnSelect: true,
            theme: "bootstrap"
        });
        $('#btnBack').click(function () {
            window.location.href = "/Competence/Index"; 
        });
        $('select[name="selectDepartment"]').on('change', function () {
            var departmentId = $(this).val();
            LoadDepartmentSection(departmentId);
        });
        $('select[name="selectLevel"]').on('change', function () { 
            var level = $(this).val(); 
            if (level != null && level != undefined && level != undefined && level != "") {
                employees = [];
                employees.push({
                    Index: 0,
                    EmployeeId: "",
                    EmployeeName: false,
                    Action: "",
                });
                employees.pop();
                RefreshExployees();
            }
        }); 
        $('#btnSaveCompetence').click(function () {
            OnSaveCompetence();
        });
        LoadCourse();
        CreateEmployeePopup(); 
        LoadJobGrades();    
        var analytic_id = $("#analytic_id").val();
        if (analytic_id != "" && analytic_id != null && analytic_id != undefined && analytic_id != "0" && analytic_id != 0) {            
            LoadCompetence(parseInt(analytic_id));
        } else {
            topics.push({
                Index: 0,
                Topic: "",
                IsSpecial: false,
                ID_Course: 0,
                Expectatoin:0,
                Action: "",
            });
            topics.pop();
            RefreshTopic();
            employees.push({
                Index: 0,
                EmployeeId: "",
                EmployeeName: "",
                Action: "",
            });
            employees.pop();
            RefreshExployees();
        }
    })
})(jQuery);

function DeleteEmployee(rowindex) {
    MessageController.ConfirmCallback("Are you sure you want to do delete this Target Employee?", "Confirm Delete Target Employee", function (res) {
        if (res) {
            employees = employees.where((item) => {
                return item.Index != rowindex;
            }); 
            var index = 1;
            employees.forEach(item => {
                item.Index = index;
                index++;
            });
            RefreshExployees();
            $("#search-target-employee-modal").magnificPopup('close');
        }
    }); 
}

function LoadCompetence(analytic_id) {
    MessageController.BlockUI({ boxed: true, textOnly: true, target: '#pn-Analytic' });
    $.ajax({
        type: "GET",
        url: "/Competence/Competence",
        data: { "competenceId": analytic_id },
        success: function (response) {
            $("#txtCompetenceName").val(response.Data.CompetenceAnalyticName);
            $("#txtExpectatoin5").val(response.Data.Criteria5);
            $("#txtExpectatoin4").val(response.Data.Criteria4);
            $("#txtExpectatoin3").val(response.Data.Criteria3);
            $("#txtExpectatoin2").val(response.Data.Criteria2);
            $("#txtExpectatoin1").val(response.Data.Criteria1);
            $("#selectLevel").val(response.Data.ID_JobGrade).trigger('change');

            var index = 1;
            $.each(response.Data.Topics, function () {
                topics.push({
                    Index: index,
                    TopicId: this.ID,
                    ID_CompetenceAnalytic: this.ID_CompetenceAnalytic,
                    Topic: this.KnowledgeTopicName,
                    IsSpecial: this.ID_Course > 0 ? false : true,
                    ID_Course: this.ID_Course,
                    Expectatoin: this.Expectatoin,
                    Action: "",
                });
                index++;
            });
            index = 1;
            $.each(response.Data.Employees, function () {
                employees.push({
                    Index: index,
                    EmployeeId: this.EmployeeID,
                    EmployeeName: this.LDMS_M_User.FullName,
                    Action: "",
                });
                index++;
            });
            RefreshExployees();
            RefreshTopic();
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
}

function CreateEmployeePopup() {
    $('.search-target-employee-modal').magnificPopup({
        type: 'inline',
        preloader: false,
        modal: true,
        callbacks: {
            beforeOpen: function () {
                LoadDepartment(); 
                LoadJobTitle(); 
                $("#selectJobGrade").removeAttr("disabled");
                var level = $("#selectLevel").val(); 
                if (level != null && level != undefined && level != undefined && level!="") {
                    $("#selectJobGrade").attr("disabled", "disabled");
                    $('select[name="selectJobGrade"]').val(level).trigger('change');
                } else {                    
                    $('select[name="selectJobGrade"]').val(null).trigger('change');
                }
                SearchEmployee();  
            },
            beforeClose: function () {
                tmpselection = []; 
            }
        }
    });

    $(document).on('click', '.popup-modal-dismiss', function (e) {
        e.preventDefault();
        $.magnificPopup.close();
    });
}

function DeleteTopic(rowindex) { 
    MessageController.ConfirmCallback("Are you sure you want to do delete this Topic?", "Confirm Delete Topic", function (res) {
        if (res) { 
            topics = topics.where((item) => {
                return item.Index != rowindex;
            });
            var index = 1;
            topics.forEach(item => {
                item.Index = index;
                index++;
            });
            RefreshTopic(); 
        } 
    }); 
}

function AddTopic(item) { 
    var index = (topics.length + 1);
    if (index > 20) {
        MessageController.Warning("Knowledge Topic maximum exceed (Maximum = 20 Topic)", "Maximum exceed");
        return;
    } 
    var text = "";
    var special = false;
    var id_course = $("#selectCourse").val();
    if (id_course != 0 && id_course != null && id_course != undefined && id_course > 0) {
        var data = $("#selectCourse").select2('data');
        text = data[0].text;  
        $('select[name="selectCourse"]').val(null).trigger('change');
        special = false;
    } else{
        text = $("#txtSpecialTopic").val();
        $("#txtSpecialTopic").val("");
        special = true;
    }    
    if (topics.length > 0) {
        var isDupicatTopicName = topics.any((item) => {
            return item.Topic.toLowerCase() == text.toLowerCase();
        });
        if (isDupicatTopicName) {
            MessageController.Warning("Knowledge Topic already exitst", "Already exitst");
            return;
        }
    }
    if (text != null && text != undefined && text != "") {
        var topic = {
            Index: index,
            TopicId:0,
            Topic: text,
            IsSpecial: special, 
            ID_Course: id_course,
            Expectatoin: 5,
            Action: "",
        }; 
        topics.push(topic); 
        RefreshTopic(); 
    } 
}

function RefreshTopic() {
    if ($.fn.dataTable.isDataTable('#dtTopic')) {
        var table = $('#dtTopic').DataTable();
        table.destroy();
    }  
   var tableTopic= $('#dtTopic').DataTable({
        'data': topics,
        'columns': [
            { data: 'Index', title:'#' },
            { data: 'Topic', title: 'Topic' },
            {
               "mData": "Expectatoin",
                "title": 'Expectatoin',
                "width":"50px",
                "mRender": function (data, type, row) {
                    return '<input type="number" id="txtExpectatoin_' + row.TopicId + '" style="width:100%;text-align:center" onkeypress="validate(event)" class="quantity"   min="0"  max="5" value="' + data+'" />';                    
                }
            },
            {
                "mData": "IsSpecial",
                title: 'Special',
                "mRender": function (data, type, row) {
                    if (data == true) {
                        return '<a style="cursor: none" ><span style="display: flex; flex-flow: row nowrap; justify-content: center;"><img src="/assets/images/svg/icon-check-green.svg" class="light-logo" alt="homepage" /></span> </a>';
                    }
                    else {
                        return '<a style="cursor: none"><span style="display: flex; flex-flow: row nowrap; justify-content: center;"><img src="/assets/images/svg/icon-cross.svg" class="light-logo" alt="homepage" /></span> </a>';
                    }
                }
            },
            {
                "mData": "Index", 
                "mRender": function(data, type, row) {
                    return '<a onclick="DeleteTopic(' + data + ')" style="cursor: pointer" style="padding-left:15px;" id="btnDeleteTopic"><span style="display: flex; flex-flow: row nowrap; justify-content: center;"><img src="/assets/images/svg/icon-delete-red.svg" class="light-logo" alt="homepage" /></span> </a>';
                }
            }
       ],
        'processing': true,
        'paging': true,
        "ordering": false,
        "searching": false,
        "lengthChange": false,
        "bAutoWidth": false,
        "Filter": false,
        "info": false,
        "bPaginate": false,
        "bLengthChange": false,
        "bJQueryUI": true,
        "sPaginationType": "full_numbers",
        "sDom": 'lfrtip',
       "pageLength": 10,
       "language": {
           "zeroRecords": "No Knowledge Topic"
       }
    });
    $('.dataTables_length').addClass('bs-select');
    tableTopic.page('last').draw('page');
}

function OnSelectEmp(element) {
    var employeeId = element.id.replace("selectEmployee_", "").trim();
    if (document.getElementById('selectEmployee_' + employeeId).checked) {
        tmpselection.push(employeeId);
    } else {
        tmpselection = tmpselection.where((item) => {
            return item != employeeId;
        }); 
    } 
}

function OnSelectEmployee() {
    if (tmpselection.length > 20) {
        MessageController.Warning("Target Employee maximum exceed (Maximum = 20 Topic)", "Maximum exceed");
        return;
    }
    employees = [];
    var table = $('#dtSelectEmployee').DataTable();
    var rowindex = 1; 
    var data = table.rows().data();  
    data.each(function (value, index) { 
        var emp = tmpselection.any((item) => {
            return item == value.EmployeeId;
        });
        if (emp) {
            employees.push({
                Index: rowindex,
                EmployeeId: value.EmployeeId,
                EmployeeName: value.EmployeeName,
                Action: "",
            });
            rowindex++;
        } 
    });
 
    RefreshExployees();
    $("#search-target-employee-modal").magnificPopup('close');
}

function RefreshExployees() {
    if ($.fn.dataTable.isDataTable('#dtEmployee')) {
        var table = $('#dtEmployee').DataTable();
        table.destroy();
    }
    var tableTopic = $('#dtEmployee').DataTable({
        'data': employees,
        'columns': [
            { data: 'Index', title: '#' },
            { data: 'EmployeeId', title: 'Employee Id' },
            { data: 'EmployeeName', title: 'Employee Name' } ,
            {
                "mData": "Index",
                "mRender": function (data, type, row) {
                    return '<a onclick="DeleteEmployee(' + data + ')" style="cursor: pointer" id="btnDeleteEmployee"><span style="display: flex; flex-flow: row nowrap; justify-content: center;"><img src="/assets/images/svg/icon-delete-red.svg" class="light-logo" alt="homepage" /></span> </a>';
                }
            }
        ], 
        'processing': true,
        'paging': true,
        "ordering": false,
        "searching": false,
        "lengthChange": false,
        "bAutoWidth": false,
        "Filter": false,
        "info": false,
        "bPaginate": false,
        "bLengthChange": false,
        "bJQueryUI": true,
        "sPaginationType": "full_numbers",
        "sDom": 'lfrtip',
        "pageLength": 20,
         "language": { 
              "zeroRecords": "No Target Employee" 
        },
    });
    $('.dataTables_length').addClass('bs-select');
    tableTopic.page('last').draw('page');
}

function LoadCourse() {
    var options = $('#selectCourse');
    options.empty();
    $.ajax({
        type: "GET",
        url: '/Master/GetAllCourses',
        success: function (response) {
            options.append($("<option />").val(null).text("Please Select"));
            $.each(response.Data, function () {
                options.append($("<option />").val(this.ID).text('(' + this.CourseID + ') ' + this.CourseName));
            });
            options.val(null).trigger('change');
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

function LoadDepartment() {
    var options = $('#selectDepartment');
    options.empty();
    $.ajax({
        type: "GET",
        url: "/Master/GetAllDepartments",
        success: function (response) {
            options.append($("<option />").val(null).text("---All---"));
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

            //options.val(null).trigger('change');
        },
        failure: function (response) {
            if (JSON.parse(response.responseText).Errors.length > 0) {
                MessageController.Error(JSON.parse(response.responseText).Errors[0].replace("Message:", ""), "Error");
            }
            else {
                MessageController.Error(response.responseText, "Error");
            }
        },
        error: function (response) {
            if (JSON.parse(response.responseText).Errors.length > 0) {
                MessageController.Error(JSON.parse(response.responseText).Errors[0].replace("Message:", ""), "Error");
            }
            else {
                MessageController.Error(response.responseText, "Error");
            }
        }
    });
}

function LoadJobGrades() {
    var options = $('#selectJobGrade');
    var options2 = $('#selectLevel');
    options.empty();
    options2.empty();

    $.ajax({
        type: "GET",
        url: "/Master/GetAllJobGrades",
        success: function (response) {
            options.append($("<option />").val(null).text("---All---"));
            options2.append($("<option />").val(null).text("---All---"));
            $.each(response.Data, function () {
                options.append($("<option />").val(this.ID_JobGrade).text(this.JobGradeName_EN));
                options2.append($("<option />").val(this.ID_JobGrade).text(this.JobGradeName_EN));
            });
            options.val(null).trigger('change');
            options2.val(null).trigger('change');
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

function LoadJobTitle() {
    var options = $('#selectJobTitle');
    options.empty();
    $.ajax({
        type: "GET",
        url: "/Master/GetAllJobTitles",
        success: function (response) {
            options.append($("<option />").val(null).text("--All--"));
            $.each(response.Data, function () {
                options.append($("<option />").val(this.ID).text(this.JobTitleName_EN));
            });
            options.val(null).trigger('change');
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

function LoadDepartmentSection(departmentId) {
    var options = $('#selectSection');
    options.empty();
    $.ajax({
        type: "GET",
        url: '/Master/GetAllSectionsByDepartment',
        data: {
            'departmentId': departmentId
        },
        success: function (response) {
            options.append($("<option />").val(null).text("---All---"));
            $.each(response.Data, function () {
                options.append($("<option />").val(this.ID_Section).text('(' + this.SectionID + ') ' + this.SectionName_EN));
            });
            options.val(null).trigger('change');
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

function SearchEmployee() {
    var searmodel = {
        EmployeeId: $("#txtEmployeeID").val(),
        EmployeeName: $("#txtEmployeeName").val(),
        Departments: $("#selectDepartment").val(),
        Sections: $("#selectSection").val(),
        JobGrades: $("#selectJobGrade").val(),
        JobTitles: $("#selectJobTitle").val() 
    }
    MessageController.BlockUI({ boxed: true, target: '#search-target-employee-modal' });
    if ($.fn.dataTable.isDataTable('#dtSelectEmployee')) {
        var table = $('#dtSelectEmployee').DataTable();
        table.destroy();
    }
    $.ajax({
        type: "GET",
        url: "/Employee/SearchEmployee",
        data: searmodel,
        success: function (response) { 
            $('#dtSelectEmployee').DataTable({
                'data': response.Data,
                'columns': [
                    { data: 'RowIndex', title: '#' },
                    { data: 'EmployeeId', title: 'Employee Id' },
                    { data: 'EmployeeName', title: 'Employee Name' },
                    { data: 'DepartmentName', title: 'Department' },
                    { data: 'SectionName', title: 'Section' },
                    { data: 'JobGrade', title: 'Job Grade' },
                    { data: 'JobTitle', title: 'Job Title' },
                    {
                        "mData": "EmployeeId",
                        "mRender": function (data, type, row) { 
                            var isSelect = employees.any((item) => {
                                return data.trim() == item.EmployeeId.trim();
                            }); 
                            if (isSelect) {
                                tmpselection.push(data);
                                return "<input type = 'checkbox' checked='checked'  onchange='OnSelectEmp(this)' class='chk-select-employee' name = 'selectEmployee_" + data + "' value='" + data + "'  id='selectEmployee_" + data + "' /> <label for='selectEmployee_" + data + "'> </label>";
                            } else {
                                return "<input type = 'checkbox' onchange='OnSelectEmp(this)' class='chk-select-employee' name = 'selectEmployee_" + data + "' value='" + data + "'  id='selectEmployee_" + data + "' /> <label for='selectEmployee_" + data + "'> </label>";
                            }
                        }
                    }
                ],  
                "columnDefs": [
                    { "orderable": false, "targets": 0, "className": "text-center", "width": "4%" },
                    { "orderable": false, "targets": 1, "className": "text-center" },
                    { "orderable": false, "targets": 2, "className": "text-center" },
                    { "orderable": false, "targets": 3, "className": "text-center" },
                    { "orderable": false, "targets": 4, "className": "text-center" },
                    { "orderable": false, "targets": 5, "className": "text-center" },
                    { "orderable": false, "targets": 6, "className": "text-center" },
                    { "orderable": false, "targets": 7, "className": "text-center" }

                ],
                'processing': true,
                'paging': true,
                "ordering": false,
                "searching": false,
                "lengthChange": false,
                "bAutoWidth": false,
                "Filter": false,
                "info": false,
                "bPaginate": false,
                "bLengthChange": false,
                "bJQueryUI": true, //Enable smooth theme
                "sPaginationType": "full_numbers", //Enable smooth theme
                "sDom": 'lfrtip',
                "pageLength": 5,
                "language": {
                    "zeroRecords": "No Employee"
                }
            });
            $('.dataTables_length').addClass('bs-select');
            MessageController.UnblockUI('#search-target-employee-modal');
        }
    });
}

function OnSaveCompetence() { 
    var competence = {
        ID_Analytic: 0,
        CompetenceAnalyticName: $("#txtCompetenceName").val(),
        Criteria1: $("#txtExpectatoin1").val(),
        Criteria2: $("#txtExpectatoin2").val(),
        Criteria3: $("#txtExpectatoin3").val(),
        Criteria4: $("#txtExpectatoin4").val(),
        Criteria5: $("#txtExpectatoin5").val(),
        ID_Analytic: 0,
        ID_EmployeeManager: CookiesController.getCookie("EMPLOYEEID"),
        ID_Department: CookiesController.getCookie("DEPARTMENTID"),
        ID_JobGrade: $("#selectLevel").val()
    };
    var employeeIds = [];
    var listTopics = [];
    employees.forEach(function (t) {
        employeeIds.push({ EmployeeID: t.EmployeeId });
    });   
    topics.forEach(function (t) {
        listTopics.push({
            ID: t.TopicId,
            ID_Course: t.ID_Course,
            KnowledgeTopicName: t.Topic,
            Expectatoin: t.Expectatoin
        })
    });

    if (competence.CompetenceAnalyticName == "" || competence.CompetenceAnalyticName == null || competence.CompetenceAnalyticName == undefined) {
        MessageController.Error("Please Enter Competence Platform Name", "Required.");
        return false;
    }
    if (listTopics.length <= 0) {
        MessageController.Error("Please Enter Knowledge Topic", "Required.");
        return false;
    }
    if (employeeIds.length <= 0) {
        MessageController.Error("Please Enter Target Employee", "Required.");
        return false;
    } 

    $('#btnSaveCompetence').attr("disabled", "disabled");
    var url = "";
    var analytic_id = $("#analytic_id").val();
    if (analytic_id != "" && analytic_id != null && analytic_id != undefined && analytic_id != "0" && analytic_id != 0) {
        url = '/Competence/UpdateCompetence';
        competence.ID_Analytic = analytic_id;

    } else {
        url = '/Competence/CreateCompetence';
    }
    $.ajax({
        type: "POST",
        url: url,
        data: {
            competenceAnalytic: competence,
            employees: employeeIds,
            topics: listTopics
        },
        success: function (response) { 
            $('#btnSaveCompetence').removeAttr("disabled");
            MessageController.Success("Save Competence Completed.", "Success");
            window.location.href = "/Competence/Index";  
        },
        failure: function (response) { 
            $('#btnSaveCompetence').removeAttr("disabled");
            if (JSON.parse(response.responseText).Errors.length > 0) {
                MessageController.Error(JSON.parse(response.responseText).Errors[0].replace("Message:", ""), "Error");
            } else {
                MessageController.Error(response.responseText, "Error");
            }
        },
        error: function (response) { 
            $('#btnSaveCompetence').removeAttr("disabled");
            if (JSON.parse(response.responseText).Errors.length > 0) {
                MessageController.Error(JSON.parse(response.responseText).Errors[0].replace("Message:", ""), "Error");
            } else {
                MessageController.Error(response.responseText, "Error");
            }
        }
    });

}

function validate(evt) {
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