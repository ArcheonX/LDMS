(function ($) {
    "use strict";
    $(document).ready(function () {
        $("input[type='datetime']").datepicker();
        $('select').select2({
            closeOnSelect: true,
            theme: "bootstrap",
            allowClear: true,
            placeholder: 'All',
            language: {
                noResults: function () {
                    return "{{ __('No results found') }}";
                }
            }
        });
        $.ajax({
            type: "GET",
            url: "/Master/GetAllDepartments",
            success: function (response) { 
                var options = $('#selectMasterReportDepartment'); 
                options.empty();
                //optionsreport.append($("<option />").val(null).text("---All---"));
                $.each(response.Data, function () { 
                    options.append($("<option />").val(this.ID_Department).text('(' + this.DepartmentID + ') ' + this.DepartmentName_EN));
                });
                $('select[name="selectMasterReportDepartment"]').val(null).trigger('change'); 
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

        $.ajax({
            type: "GET",
            url: '/Master/GetAllPlatforms',
            success: function (response) {
                var options = $('#selectPlatform');
                options.empty();
                //options.append($("<option />").val(null).text("---All---"));
                $.each(response.Data, function () {
                    options.append($("<option />").val(this.ID).text('(' + this.PlatformID + ') ' + this.PlatformName_EN));
                });
                $('select[name="selectPlatform"]').val(null).trigger('change');
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

        $.ajax({
            type: "GET",
            url: '/Master/GetAllCourses',
            success: function (response) {
                var options = $('#selectCourse');
                options.empty();
                //options.append($("<option />").val(null).text("---All---"));
                $.each(response.Data, function () {
                    options.append($("<option />").val(this.ID).text('(' + this.CourseID + ') ' + this.CourseName));
                });
                $('select[name="selectCourse"]').val(null).trigger('change');
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

        $.ajax({
            type: "GET",
            url: "/Master/GetAllJobGrades",
            success: function (response) {
                var options = $('#selectMasterReportJobGrade');
                //options.append($("<option />").val(null).text("---All---"));
                $.each(response.Data, function () {
                    options.append($("<option />").val(this.ID_JobGrade).text(this.JobGradeName_EN));
                });
                $('select[name="selectMasterReportJobGrade"]').val(null).trigger('change');
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
         
        $('#btnExportMasterReport').click(function () {
            event.preventDefault();
            //if (!$("#reportFrm").valid()) {
            //    return false;
            //}
            if ($("#dteTrainingDateFrm").val() == null || $("#dteTrainingDateFrm").val() == "" || $("#dteTrainingDateFrm").val() == undefined) {
                MessageController.Error("Please Enter Training Date?", "Validator");
                return;
            }
            if ($("#dteTrainingDateTo").val() == null || $("#dteTrainingDateTo").val() == "" || $("#dteTrainingDateTo").val() == undefined) {
                MessageController.Error("Please Enter Training Date?", "Validation failed.");
                return;
            }
            ExportMasterReport($);
        });
    })
})(jQuery);

function ExportMasterReport($) {
    var filterModel = {
        MasterReportType: $('input[name="selectMasterType"]:checked').val(),
        InstructorId: $("#txtInstructorId").val(),
        CourseId: $("#selectCourse").val(),
        PlatformId: $("#selectPlatform").val(),
        ActiveStatus: $("#selectMasterReportStatus").val(),
        TrainingStatus: $("#selectMasterReportTrainingStatus").val(),
        JobGradeId: $("#selectMasterReportJobGrade").val(),
        DepartmentId: $("#selectMasterReportDepartment").val(),
        TrainingDateFrm: $("#dteTrainingDateFrm").val(),
        TrainingDateTo: $("#dteTrainingDateTo").val()
    }
    $.ajax({
        type: "GET",
        url: '/Report/GetIMasterReport',
        data: filterModel,
        success: function (response) {
            var reportFile = Utility.base64ToArrayBuffer(response.Data.FileContents);
            Utility.ExportExcelFile(response.Data.FileDownloadName, reportFile);
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
};

function CreateDataTablePaging() {
    if ($.fn.dataTable.isDataTable('#dtUserList')) {
        var table = $('#dtUserList').DataTable();
        table.destroy();
    }
    $('#dtUserList').DataTable({
        'processing': true,
        'paging': true,
        "ordering": true,
        "searching": false,
        "lengthChange": false,
        "bAutoWidth": false,
        "Filter": false,
        "info": false,
        "bPaginate": false,
        "bLengthChange": false,
        "pageLength": 10,
        "bJQueryUI": true, //Enable smooth theme
        "sPaginationType": "full_numbers", //Enable smooth theme
        "sDom": 'lfrtip'
    });
    $('.dataTables_length').addClass('bs-select');
}

function OnChangeMasterType(ele) {
    $("#txtInstructorId").attr("disabled", "disabled");
    $("#selectPlatform").attr("disabled", "disabled");
    $("#selectCourse").attr("disabled", "disabled");

    if (ele.id == "Instructor") {
        $("#txtInstructorId").removeAttr("disabled");
    } else if (ele.id == "Platform") {
        $("#selectPlatform").removeAttr("disabled");
    } else if (ele.id == "Course") {
        $("#selectCourse").removeAttr("disabled");
    }
}

