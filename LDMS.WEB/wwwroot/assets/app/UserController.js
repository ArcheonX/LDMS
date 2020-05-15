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

        var validobj = $("#frmUserEditor").validate({ 
            onkeyup: false,
            errorClass: "myErrorClass",
            highlight: function (element, errorClass, validClass) {
                var elem = $(element); 
                elem.addClass(errorClass); 
            },
            unhighlight: function (element, errorClass, validClass) {
                var elem = $(element); 
                elem.removeClass(errorClass); 
            },
            errorPlacement: function (error, element) {
                var elem = $(element); 
                error.insertAfter(elem);
                error.attr("style", "color: #f60b0b");
            },
        }); 

        $.ajax({
            type: "GET",
            url: "/Master/GetAllDepartments",
            success: function (response) {
                var options = $('#selectFilterDepartment');
                options.append($("<option />").val(null).text("---All---")); 
                $.each(response.Data, function () {
                    options.append($("<option />").val(this.ID_Department).text('(' + this.DepartmentID + ') ' + this.DepartmentName_EN));
                });
                $('select[name="selectFilterDepartment"]').val(null).trigger('change');
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

        SearchEmployee($);

        $('select[name="selectCenter"]').on('change', function () {
            if (!$.isEmptyObject(validobj.submitted)) {
                validobj.form();
            }
            var centerId = $(this).val();
            ReloadDivision($, centerId, null);
        }); 

        $('select[name="selectDivision"]').on('change', function () { 
            var divisionId = $(this).val();
            ReloadDepartment($, divisionId, null); 
        });

        $('select[name="selectDepartment"]').on('change', function () { 
            var departmentId = $(this).val();
            ReloadSection($, departmentId, null);  
        }); 

        $('select[name="selectJobGrade"]').on('change', function () {
            if (!$.isEmptyObject(validobj.submitted)) {
                validobj.form();
            }
        });

        $('select[name="selectJObTitle"]').on('change', function () {
            if (!$.isEmptyObject(validobj.submitted)) {
                validobj.form();
            }
        });

        $('select[name="selectGender"]').on('change', function () {
            if (!$.isEmptyObject(validobj.submitted)) {
                validobj.form();
            }
        }); 

        $('#btnResetPassword').click(function () {
            MessageController.ConfirmCallback("Are you sure you want to reset to default password?", "Confirm change", function (res) {
                if (res) {
                    $.ajax({
                        type: "POST",
                        url: '/Employee/ResetPassword',
                        data: { 'employeeId': $("#txtEmployeeId").val() },
                        success: function (response) { 
                            MessageController.Success("Reset password success.", "Success");
                            SearchEmployee();
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
            });
        });

        $('#btnUser').click(function () {
            $('#viewAllUser').attr("style", "display:block;width:100%");
            $('#UserEditor').attr("style", "display:none;width:100%");
            $('#MasterReport').attr("style", "display:none;width:100%");
            $('#btnUser').attr("class", "btn btn-info waves-effect waves-light");
            $('#btnMasterReport').attr("class", "btn btn btn-outline-info waves-effect waves-light");
        });

        $('#btnMasterReport').click(function () {
            $('#viewAllUser').attr("style", "display:none;width:100%");
            $('#UserEditor').attr("style", "display:none;width:100%");
            $('#MasterReport').attr("style", "display:block;width:100%");
            $('#btnUser').attr("class", "btn btn btn-outline-info waves-effect waves-light");
            $('#btnMasterReport').attr("class", "btn btn-info waves-effect waves-light");
        });

        $('#btnAddEmployee').click(function () { 
            $('#viewAllUser').attr("style", "display:none;width:100%");
            $('#UserEditor').attr("style", "display:block;width:100%");
            $('#MasterReport').attr("style", "display:none;width:100%");
            CreateEditor(); 
        });

        $('#btnBack').click(function () {  
            $('#viewAllUser').attr("style", "display:block;width:100%");
            $('#UserEditor').attr("style", "display:none;width:100%");
            $('#MasterReport').attr("style", "display:none;width:100%");
        });

        $('#btnSaveEmployee').click(function (event) {
            event.preventDefault();
            if (!$("#frmUserEditor").valid()) {
                return false;
            }
            var roleId = 0;
            $('#dtUserRoleList > tbody  > tr').each(function () {
                var self = $(this);
                var isSelected = self.find("td").eq(3).find("input[type='radio']:checked");
                if (isSelected && isSelected.prop('checked')) {
                    debugger;
                    var value = isSelected.val();
                    roleId = parseInt(value);
                }
            });
            if (roleId == null || roleId == undefined || roleId == 0) {
                MessageController.Error("Please Enter User Role", "User Role Required");
                return false;
            }
            MessageController.ConfirmCallback("Are you sure you want to do this?", "Confirm change", function (res) {
                if (!res) {
                    return false;
                }
                
                var empModel = {
                    CenterId: parseInt($("#selectCenter").val()),
                    DivisionId: parseInt($("#selectDivision").val()),
                    DepartmentId: parseInt($("#selectDepartment").val()),
                    SectionId: parseInt($("#selectSection").val()),
                    JobGradeId: parseInt($("#selectJobGrade").val()),
                    JobTitleId: parseInt($("#selectJObTitle").val()),
                    Gender: $("#selectGender").val(),
                    IsEditMode: $("#userEditMode").val() == "true",
                    EmployeeId: $("#txtEmployeeId").val(),
                    EmployeeName: $("#txtEmployeeName").val(),
                    EmployeeSurName: $("#txtEmployeeSurName").val(), 
                    Nationality: $("#txtNationality").val(), 
                    Email: $("#txtEmail").val(), 
                    Phone: $("#txtPhone").val(), 
                    Password: $("#txtPassword").val(),
                    Remark: $("#txtRemark").val(),
                    RoleId: roleId,
                    IsInstructer: $("#isInstructer").val() == "true",
                    IsSectionHead: $("#isSectionHead").val() == "true",
                    Id_Employee: $("#Id_Employee").val() 
                }; 
                if ($("#userEditMode").val() == "true") {
                    UpdateEmployee(empModel);
                } else {
                    CreateEmployee(empModel);
                }  
           }); 
        }); 

        $('#btnSearchEmployee').click(function () { 
            SearchEmployee();
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

function ReloadDivision($, centerId, callback) {
    var options = $('#selectDivision');
    options.empty(); 
    $.ajax({
        type: "GET",
        url: "/Master/GetAllDivisionsByCenter",
        data: {
            'centerId': centerId
        },
        success: function (response) {
            options.append($("<option />").val(null).text("Please select")); 
            $.each(response.Data, function () {
                options.append($("<option />").val(this.ID_Division).text('(' + this.DivisionID + ') ' + this.DivisionName_EN));
            }); 
            if (callback) {
                callback();
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

function ReloadDepartment($, divisionId, callback) {
    var options = $('#selectDepartment');
    options.empty();
    $.ajax({
        type: "GET",
        url: "/Master/GetAllDepartmentsByDivision",
        data: {
            'divisionId': divisionId
        },
        success: function (response) {

            options.append($("<option />").val(null).text("Please select"));
            $.each(response.Data, function () {
                options.append($("<option />").val(this.ID_Department).text('(' + this.DepartmentID + ') ' + this.DepartmentName_EN));
            });
            options.val($("#userDepartmentId").val()).trigger('change');
            if (callback) {
                callback($("#userDepartmentId").val());
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

function ReloadSection($, departmentId, callback) { 
    var options = $('#selectSection');
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
            options.val($("#userSectionId").val()).trigger('change');
            if (callback) {
                callback($("#userSectionId").val());
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

function UpdateEmployee(empModel) {
    $('#btnSaveEmployee').attr("disabled", "disabled");
    $.ajax({
        type: "POST",
        url: '/Employee/UpdateEmployee',
        data: empModel,
        success: function (response) {
            $('#btnSaveEmployee').removeAttr("disabled");
            $('#viewAllUser').attr("style", "border-width:thin;border-style:solid;display:block;width:100%");
            $('#UserEditor').attr("style", "border-width:thin;border-style:solid;display:none;width:100%");
            $('#MasterReport').attr("style", "border-width:thin;border-style:solid;display:none;width:100%"); 
            SearchEmployee(); 
        },
        failure: function (response) {
            $('#btnSaveEmployee').removeAttr("disabled");
            if (JSON.parse(response.responseText).Errors.length > 0) {
                MessageController.Error(JSON.parse(response.responseText).Errors[0].replace("Message:", ""), "Error");
            } else {
                MessageController.Error(response.responseText, "Error");
            }
        },
        error: function (response) {
            $('#btnSaveEmployee').removeAttr("disabled");
            if (JSON.parse(response.responseText).Errors.length > 0) {
                MessageController.Error(JSON.parse(response.responseText).Errors[0].replace("Message:", ""), "Error");
            } else {
                MessageController.Error(response.responseText, "Error");
            }
        }
    });
}

function CreateEmployee(empModel) {
    $('#btnSaveEmployee').attr("disabled", "disabled");
    $.ajax({
        type: "POST",
        url: '/Employee/CreateEmployee',
        data: empModel,
        success: function (response) {
            $('#btnSaveEmployee').removeAttr("disabled");
            $('#viewAllUser').attr("style", "border-width:thin;border-style:solid;display:block;width:100%");
            $('#UserEditor').attr("style", "border-width:thin;border-style:solid;display:none;width:100%");
            $('#MasterReport').attr("style", "border-width:thin;border-style:solid;display:none;width:100%"); 
            SearchEmployee();
        },
        failure: function (response) {
            $('#btnSaveEmployee').removeAttr("disabled");
            if (JSON.parse(response.responseText).Errors.length > 0) {
                MessageController.Error(JSON.parse(response.responseText).Errors[0].replace("Message:", ""), "Error");
            } else {
                MessageController.Error(response.responseText, "Error");
            }
        },
        error: function (response) {
            $('#btnSaveEmployee').removeAttr("disabled");
            if (JSON.parse(response.responseText).Errors.length > 0) {
                MessageController.Error(JSON.parse(response.responseText).Errors[0].replace("Message:", ""), "Error");
            } else {
                MessageController.Error(response.responseText, "Error");
            }
        }
    });
}

function DeleteEmployee(employeeId) {
    MessageController.WarningCallback("Are you sure you want to delete Employee '" + employeeId + "'?", "Confirm Delete!", function (res) {
        if (res) {
            $.ajax({
                type: "Delete",
                url: '/Employee/RemoveEmployee',
                data: { 'employeeId': employeeId },
                success: function (response) {
                    SearchEmployee();
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
    });
}

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
}
function EditEmployee(employeeId) {

    $('#viewAllUser').attr("style", "display:none;width:100%");
    $('#UserEditor').attr("style", "display:block;width:100%");
    $('#MasterReport').attr("style", "display:none;width:100%");
    ClearEditor();

    $("#userEditMode").val("false");
    $('select[name="selectDivision"]').val(null);
    $('select[name="selectDepartment"]').val(null);
    $('select[name="selectSection"]').val(null);
    $('select[name="selectGender"]').val(null);
    $("#userCenterId").val(null);
    $("#userDivisionId").val(null);
    $("#userDepartmentId").val(null);
    $("#userSectionId").val(null);
    $("#btnResetPassword").attr("style", "display:none;width:100%");
    $("#isInstructer").prop("checked", false);
    $("#isSectionHead").prop("checked", false);
    $("#Id_Employee").val(0);

    if (employeeId != undefined && employeeId != "") {
        MessageController.BlockUI({ boxed: true, target: '#UserEditor' });

        $.ajax({
            type: "GET",
            url: "/Employee/ReadEmployee",
            data: { 'employeeId': employeeId },
            success: function (response) {
                $("#userEditMode").val("true");
                var user = response.Data;
                $("#userCenterId").val(user.CenterId);
                $("#userDivisionId").val(user.DivisionId);
                $("#userDepartmentId").val(user.DepartmentId);
                $("#userSectionId").val(user.SectionId);
                $("#Id_Employee").val(user.Id_Employee);


                if (user.IsAD == false) {
                    $("#btnResetPassword").attr("style", "display:block;width:100%");
                }
                if (user.IsInstructer == true) {
                    $("#isInstructer").prop("checked", true);
                }

                if (user.IsSectionHead == true) {
                    $("#isSectionHead").prop("checked", true);
                }

                $.ajax({
                    type: "GET",
                    url: "/Master/GetAllJobGrades",
                    success: function (response) {
                        var options = $('#selectJobGrade');
                        options.append($("<option />").val(null).text("Please select"));
                        $.each(response.Data, function () {
                            options.append($("<option />").val(this.ID_JobGrade).text(this.JobGradeName_EN));
                        });
                        $('#selectJobGrade').val(user.JobGradeId).trigger('change');
                    } 
                });

                $.ajax({
                    type: "GET",
                    url: "/Master/GetAllJobTitles",
                    success: function (response) {
                        var options = $('#selectJObTitle');
                        options.append($("<option />").val(null).text("Please select"));
                        $.each(response.Data, function () {
                            options.append($("<option />").val(this.ID_JobTitle).text(this.JobTitleName_EN));
                        });
                        $('#selectJObTitle').val(user.JobTitleId).trigger('change');
                    } 
                }); 
                $.ajax({
                    type: "GET",
                    url: "/Master/GetAllRoles",
                    success: function (response) {
                        if ($.fn.dataTable.isDataTable('#dtUserRoleList')) {
                            var table = $('#dtUserRoleList').DataTable();
                            table.destroy();
                        }
                        $('#dtUserRoleList').DataTable({
                            'data': response.Data,
                            'columns': [
                                { "data": 'RowIndex', title: '#' },
                                { "data": 'RoleName_EN', title: 'Role Name' },
                                { "data": 'RoleDescription', title: 'Description' },
                                {
                                    "mData": "ID_Role",
                                    "bSortable": false,
                                    "mRender": function (data, type, row) { 
                                        if (data == user.RoleId) {
                                            return '<input type="radio" name="selectUserRole" checked="checked" value="' + data + '"  id="selectRole_' + data + '" /><label for="selectRole_' + data + '"> </label>'
                                        } else {
                                            return '<input type="radio" name="selectUserRole"  value="' + data + '"  id="selectRole_' + data + '" /><label for="selectRole_' + data + '"> </label>'
                                        }
                                    }
                                }
                            ],
                            "columnDefs": [
                                { "orderable": true, "targets": 0, "className": "text-center" },
                                { "orderable": true, "targets": 1, "className": "text-left" },
                                { "orderable": true, "targets": 2, "className": "text-left" },
                                { "orderable": true, "targets": 3, "className": "text-center" }
                            ],
                            'processing': true,
                            'paging': false,
                            "ordering": true,
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
                            "language": {
                                "zeroRecords": "No Role"
                            }
                        });
                        $('.dataTables_length').addClass('bs-select');  
                    } 
                });

                $.ajax({
                    type: "GET",
                    url: "/Master/GetAllCenters",
                    success: function (response) {
                        var options = $('#selectCenter');
                        options.append($("<option />").val(null).text("Please select"));
                        $.each(response.Data, function () {
                            options.append($("<option />").val(this.ID_Center).text(this.CenterName_EN));
                        });
                        $('#selectCenter').val(user.CenterId).trigger('change');

                        $.ajax({
                            type: "GET",
                            url: "/Master/GetAllDivisionsByCenter",
                            data: {
                                'centerId': user.CenterId
                            },
                            success: function (response) {
                                var Divisionoptions = $('#selectDivision');
                                Divisionoptions.empty();
                                Divisionoptions.append($("<option />").val(null).text("Please select"));
                                $.each(response.Data, function () {
                                    Divisionoptions.append($("<option />").val(this.ID_Division).text('(' + this.DivisionID + ') ' + this.DivisionName_EN));
                                });
                                $('#selectDivision').val(user.DivisionId).trigger('change');

                                $.ajax({
                                    type: "GET",
                                    url: "/Master/GetAllDepartmentsByDivision",
                                    data: {
                                        'divisionId': user.DivisionId
                                    },
                                    success: function (response) {
                                        var Departmentoptions = $('#selectDepartment');
                                        Departmentoptions.empty();
                                        Departmentoptions.append($("<option />").val(null).text("Please select"));
                                        $.each(response.Data, function () {
                                            Departmentoptions.append($("<option />").val(this.ID_Department).text('(' + this.DepartmentID + ') ' + this.DepartmentName_EN));
                                        });
                                        $('#selectDepartment').val(user.DepartmentId).trigger('change'); 
                                        $.ajax({
                                            type: "GET",
                                            url: '/Master/GetAllSectionsByDepartment',
                                            data: {
                                                'departmentId': user.DepartmentId
                                            },
                                            success: function (response) {

                                                var Sectionoptions = $('#selectSection');
                                                Sectionoptions.empty();

                                                Sectionoptions.append($("<option />").val(null).text("Please select"));
                                                $.each(response.Data, function () {
                                                    Sectionoptions.append($("<option />").val(this.ID_Section).text('(' + this.SectionID + ') ' + this.SectionName_EN));
                                                });
                                                $('#selectSection').val(user.SectionId).trigger('change');
                                            }
                                        });
                                    }
                                });
                            }
                        });
                    }
                });

                $('#selectGender').val(user.Gender).trigger('change');
                $('#txtEmployeeId').val(user.EmployeeId);
                $('#txtEmployeeName').val(user.EmployeeName);
                $('#txtEmployeeSurName').val(user.EmployeeSurName);
                $('#txtNationality').val(user.Nationality);
                $('#txtEmail').val(user.Email);
                $('#txtPhone').val(user.Phone);
                $('#txtRemark').val(user.Remark);
                $('#isInstructer').val(user.IsInstructer);
                $('#isSectionHead').val(user.IsSectionHead); 
                MessageController.UnblockUI('#UserEditor');
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
}

function ClearEditor() {
    $("#userCenterId").val("");
    $("#userDivisionId").val("");
    $("#userDepartmentId").val("");
    $("#userSectionId").val("");
    $("#isInstructer").prop("checked", false);
    $("#btnResetPassword").attr("style", "display:none;width:100%"); 
    $("#isSectionHead").prop("checked", false);
    $('#selectCenter').val(null).trigger('change');  
    $('#selectGender').val(null).trigger('change');
    $('#selectJobGrade').val(null).trigger('change');
    $('#selectJObTitle').val(null).trigger('change');
    $('#selectDivision').val(null).trigger('change');
    $('#selectDepartment').val(null).trigger('change');
    $('#selectSection').val(null).trigger('change');   
    $('#txtEmployeeId').val("");
    $('#txtEmployeeName').val("");
    $('#txtEmployeeSurName').val("");
    $('#txtNationality').val("");
    $('#txtEmail').val("");
    $('#txtPhone').val("");
    $('#txtRemark').val("");
    $('#dtUserRoleList > tbody  > tr').each(function () {
        var self = $(this); 
        var check = self.find("td").eq(4).find("input[type='radio']");
        check.prop("checked", false);
    }); 
}
 
function CreateEditor() {
    ClearEditor();
    $("#userEditMode").val("false");
    $('select[name="selectDivision"]').val(null);
    $('select[name="selectDepartment"]').val(null);
    $('select[name="selectSection"]').val(null);
    $('select[name="selectGender"]').val(null);
    $("#userCenterId").val(null);
    $("#userDivisionId").val(null);
    $("#userDepartmentId").val(null);
    $("#userSectionId").val(null);
    $("#btnResetPassword").attr("style", "display:none;width:100%");
    $("#isInstructer").prop("checked", false);
    $("#isSectionHead").prop("checked", false);
    $("#Id_Employee").val(0);
    $.ajax({
        type: "GET",
        url: "/Master/GetAllJobGrades",
        success: function (response) {
            var options = $('#selectJobGrade');
            options.append($("<option />").val(null).text("Please select"));
            $.each(response.Data, function () {
                options.append($("<option />").val(this.ID_JobGrade).text(this.JobGradeName_EN));
            });
            $('select[name="selectJobGrade"]').val(null).trigger('change');
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
        url: "/Master/GetAllJobTitles",
        success: function (response) {
            var options = $('#selectJObTitle');
            options.append($("<option />").val(null).text("Please select"));
            $.each(response.Data, function () {
                options.append($("<option />").val(this.ID_JobTitle).text(this.JobTitleName_EN));
            });
            $('select[name="selectJObTitle"]').val(null).trigger('change');
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
        url: "/Master/GetAllCenters",
        success: function (response) {
            var options = $('#selectCenter');
            options.append($("<option />").val(null).text("Please select"));
            $.each(response.Data, function () {
                options.append($("<option />").val(this.ID_Center).text(this.CenterName_EN));
            });
            $('select[name="selectCenter"]').val(null).trigger('change');
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
        url: "/Master/GetAllRoles",
        success: function (response) {
            if ($.fn.dataTable.isDataTable('#dtUserRoleList')) {
                var table = $('#dtUserRoleList').DataTable();
                table.destroy();
            }
            $('#dtUserRoleList').DataTable({
                'data': response.Data,
                'columns': [
                    { "data": 'RowIndex', title: '#' },
                    { "data": 'RoleName_EN', title: 'Role Name' },
                    { "data": 'RoleDescription', title: 'Description' },  
                    {
                        "mData": "ID_Role",
                        "bSortable": false,
                        "mRender": function (data, type, row) {
                            if (data == 1) {
                                return '<input type="radio" name="selectUserRole" checked="checked" value="' + data + '"  id="selectRole_' + data + '"  /><label for="selectRole_' + data + '"> </label>'
                            } else {
                                return '<input type="radio" name="selectUserRole"  value="' + data + '"  id="selectRole_' + data + '"   /><label for="selectRole_' + data + '"> </label>'
                            }
                        }
                    }
                ],
                "columnDefs": [
                    { "orderable": true, "targets": 0, "className": "text-center" },
                    { "orderable": true, "targets": 1, "className": "text-left" },
                    { "orderable": true, "targets": 2, "className": "text-left" },
                    { "orderable": true, "targets": 3, "className": "text-center" }
                ],
                'processing': true,
                'paging': false,
                "ordering": true,
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
                "language": {
                    "zeroRecords": "No Role"
                }
            });
            $('.dataTables_length').addClass('bs-select'); 
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
    MessageController.BlockUI({ boxed: true, target: '#dtUserList' });
    var deps = $('#selectFilterDepartment').val();
    var searmodel = {
        EmployeeId: $("#txtFilterEmployeeID").val(),
        EmployeeName: $("#txtFilterEmployeeName").val(),
        Departments: deps != null && deps != undefined ? deps.join(",") : null,
        Sections: null,
        JobGrades: null,
        JobTitles: null 
    }; 
    $.ajax({
        type: "GET",
        url: "/Account/SearchEmployee",
        data: searmodel,
        success: function (response) {  
            $('#UserListcontent').empty();
            $('#UserListcontent').html(response);
            CreateDataTablePaging(); 
            MessageController.UnblockUI('#dtUserList');
        },
        failure: function (response) {
            MessageController.UnblockUI('#dtUserList');
            if (JSON.parse(response.responseText).Errors.length > 0) {
                MessageController.Error(JSON.parse(response.responseText).Errors[0].replace("Message:", ""), "Error");
            } else {
                MessageController.Error(response.responseText, "Error");
            }
        },
        error: function (response) {
            MessageController.UnblockUI('#dtUserList');
            if (JSON.parse(response.responseText).Errors.length > 0) {
                MessageController.Error(JSON.parse(response.responseText).Errors[0].replace("Message:", ""), "Error");
            } else {
                MessageController.Error(response.responseText, "Error");
            }
        }
    });
}

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