var IsInit = true;

(function ($) {
    "use strict";
    $(document).ready(function () {  
    $('select').select2({
        allowClear: true,
        closeOnSelect: true,
        theme: "bootstrap"
    }); 
        $('#btnEditSection').click(function () {
            $('#viewAllUser').attr("style", "width: 100%; margin-top: -15px; display: none;");
            $('#editSection').attr("style", "width: 100%; margin-top: -15px; display: block;");
            LoadSection();
        });
        $('#btnBack').click(function () {
            $('#viewAllUser').attr("style", "width: 100%; margin-top: -15px; display: block;");
            $('#editSection').attr("style", "width: 100%; margin-top: -15px; display: none;");
            LoadDepartmentSection();
        });

        $("#btnClear").click(function () {
            MessageController.WarningCallback("Are you sure you want to dicard changes'?", "Confirm Delete!", function (res) {
                if (res) {
                    $("#txtSectionId").val(null);
                    $("#txtSectionName").val(null);
                }
            })
        });
        //$("#btnAllowImport").click(function () {
        //    console.log("AA");
        //    //MessageController.WarningCallback("Are you sure you want to dicard changes'?", "Confirm Delete!", function (res) {
        //    //    if (res) {
        //    //        $("#txtSectionId").val(null);
        //    //        $("#txtSectionName").val(null);
        //    //    }
        //    //})
        //});

        $("#btnCancel").click(function (event) {
            MessageController.ConfirmCallback("Are you sure you want to cancel changes", "Confirm Cancel Change", function (res) {
                if (!res) {
                    return false;
                } 
                LoadEmployees();
            });
        });
        $('#btnSavSection').click(function (event) {
            event.preventDefault();
            if (!$("#frmSectionEditor").valid()) {
                return false;
            }
            if ($("#txtSectionName").val() == undefined || $("#txtSectionName").val() == null || $("#txtSectionName").val() == "") {
                MessageController.Error("Please Enter Section Name", "Validation Failed");
                return false;
            }

            if ($("#txtSectionAbbreviation").val() == undefined || $("#txtSectionAbbreviation").val() == null || $("#txtSectionAbbreviation").val() == "") {
                MessageController.Error("Please Enter Section Abbreviation", "Validation Failed");
                return false;
            }
            MessageController.ConfirmCallback("Are you sure you want to do this?", "Confirm change", function (res) {
                if (!res) {
                    return false;
                }
                var model = {
                    ID_Section: $("#txtSectionId").val(),
                    SectionID: $("#txtSectionAbbreviation").val(),
                    ID_Department: CookiesController.getCookie("DEPARTMENTID"),
                    SectionName_EN: $("#txtSectionName").val(),
                    SectionName_TH: $("#txtSectionName").val(),
                    Description: "" 
                }
                if (model.ID_Section <= 0
                    || model.ID_Section == null
                    || model.ID_Section == "null"
                    || model.ID_Section == undefined) {
                    model.ID_Section = 0;
                    CreateSection(model);
                } else {
                    UpdateSection(model);
                }
            });
        });
        $("#btnSearchEmployee").click(function (event) {
            LoadEmployees();
        }); 
      $("#btnExportExcel").click(function (event) {
            ExportEmployees();
        }); 

        $("#btnEmpListSave").click(function (event) {
            var models = []; 
            var table = $('#dtListEmployee').DataTable(); 
            var data = table.rows().data();
            data.each(function (value, index) { 
                var sectionHead = $("#selectSectionHead_" + value[1]).prop('checked'); // document.Find self.find("td").eq(5).find("input[type='checkbox']").prop("checked");
                var sectionId = $("#selectSection_" + value[1]).val();// self.find("td").eq(6).find("select option:selected").val();
                var model = {
                    EmployeeId: value[1],
                    IsSectionHead: sectionHead,
                    SectionId: sectionId 
                };
                models.push(model); 
            });

            $.ajax({
                type: "POST",
                url: "/Organization/SectionEmployeeSave",
                data: {
                    models: models
                },
                success: function (response) {
                    MessageController.Success("Update Completed.", "Success");
                    LoadEmployees();
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
        });  
        $('.custom-file input').change(function (e) {
            if (e.target.files.length) {
                $(this).next('.custom-file-label').html(e.target.files[0].name);
            }
            uploadFiles();
        });
    $.ajax({
        type: "GET",
        url: "/Master/Division",
        data: {
            'divisionId': CookiesController.getCookie("DIVISIONID")
        },
        success: function (response) { 
            $("#txtDivision").val(response.Data.DivisionID + " " + response.Data.DivisionName_EN); 
            $("#txtEditDivision").val(response.Data.DivisionID + " " + response.Data.DivisionName_EN); 
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
        url: "/Master/Department",
        data: {
            'departmentId': CookiesController.getCookie("DEPARTMENTID")
        },
        success: function (response) { 
            $("#txtDepartment").val(response.Data.DepartmentID + " " + response.Data.DepartmentName_EN); 
            $("#txtEditDepartment").val(response.Data.DepartmentID + " " + response.Data.DepartmentName_EN); 
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
        LoadEmployees(); 
        LoadDepartmentSection();  
        CreatePopupImportFile();
    })
})(jQuery);

function ExportEmployees() { 
    $.ajax({
        type: "GET",
        url: '/Organization/ExportEmployees',
        data: {
            'departmentId': CookiesController.getCookie("DEPARTMENTID"),
            'sectionId': $("#selectSection").val(),
            'keyword': $("#txtKeyword").val()
        },
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
function LoadSection() {
    MessageController.BlockUI({ boxed: true, target: '#dtSectionRows' });
    $.ajax({
        type: "GET",
        url: '/Organization/Sections',
        data: {
            'departmentId': CookiesController.getCookie("DEPARTMENTID")
        },
        success: function (response) {
            $('#dtSectionRows').empty();
            $('#dtSectionRows').html(response);
            try {
                if ($.fn.dataTable.isDataTable('#dtSectionRows')) {
                    var table = $('#dtSectionRows').DataTable();
                    table.destroy();
                }
                $('#dtSectionRows').DataTable({
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
                    "bJQueryUI": true, //Enable smooth theme
                    "sPaginationType": "full_numbers", //Enable smooth theme
                    "sDom": 'lfrtip',
                    "pageLength": 10,
                    "language": {
                        "zeroRecords": "No data"
                    }
                });
                $('.dataTables_length').addClass('bs-select');
                CreateSectionPopup();
            } catch (e) {
                return false;
            }
            MessageController.UnblockUI('#dtSectionRows');
        },
        failure: function (response) {
            MessageController.UnblockUI('#dtSectionRows');
            if (JSON.parse(response.responseText).Errors.length > 0) {
                MessageController.Error(JSON.parse(response.responseText).Errors[0].replace("Message:", ""), "Error");
            } else {
                MessageController.Error(response.responseText, "Error");
            }
        },
        error: function (response) {
            MessageController.UnblockUI('#dtSectionRows');
            if (JSON.parse(response.responseText).Errors.length > 0) {
                MessageController.Error(JSON.parse(response.responseText).Errors[0].replace("Message:", ""), "Error");
            } else {
                MessageController.Error(response.responseText, "Error");
            }
        }
    });
}

function LoadEmployees() {
    MessageController.BlockUI({ boxed: true, target: '#dtListEmployee' });
    $.ajax({
        type: "GET",
        url: '/Organization/Employees',
        data: {
            'departmentId': CookiesController.getCookie("DEPARTMENTID"),
            'sectionId': $("#selectSection").val(),
            'keyword': $("#txtKeyword").val()
        },
        success: function (response) {
            $('#dtListEmployee').empty();
            $('#dtListEmployee').html(response);
            try {
                if ($.fn.dataTable.isDataTable('#dtListEmployee')) {
                    var table = $('#dtListEmployee').DataTable();
                    table.destroy();
                }
                $('#dtListEmployee').DataTable({
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
                    "bJQueryUI": true, //Enable smooth theme
                    "sPaginationType": "full_numbers", //Enable smooth theme
                    "sDom": 'lfrtip'
                });
                $('.dataTables_length').addClass('bs-select');
                IsInit = false;
            } catch (e) {
                return false;
            }
            MessageController.UnblockUI('#dtListEmployee');
        },
        failure: function (response) {
            MessageController.UnblockUI('#dtListEmployee');
            if (JSON.parse(response.responseText).Errors.length > 0) {
                MessageController.Error(JSON.parse(response.responseText).Errors[0].replace("Message:", ""), "Error");
            } else {
                MessageController.Error(response.responseText, "Error");
            }
        },
        error: function (response) {
            MessageController.UnblockUI('#dtListEmployee');
            if (JSON.parse(response.responseText).Errors.length > 0) {
                MessageController.Error(JSON.parse(response.responseText).Errors[0].replace("Message:", ""), "Error");
            } else {
                MessageController.Error(response.responseText, "Error");
            }
        }
    });
}

function LoadDepartmentSection() {
    $.ajax({
        type: "GET",
        url: '/Organization/SectionByDepartment',
        data: {
            'departmentId': CookiesController.getCookie("DEPARTMENTID")
        },
        success: function (response) {
            var options = $('#selectSection');
            options.empty();
            options.append($("<option />").val(null).text("---All---"));
            $.each(response.Data, function () { 
                options.append($("<option />").val(this.ID_Section).text('(' + this.SectionID + ') ' + this.SectionName_EN));
            });
            Array.prototype.slice.call(document.querySelectorAll('select[id*="selectSection"]'))
                .forEach(function (element) { 
                    if (element.id != 'selectSection') {
                        var selectValue = element.value;
                        var options2 = $("#" + element.id);
                        options2.empty();
                        options2.append($("<option />").val(null).text("---None---"));
                        $.each(response.Data, function () {
                            options2.append($("<option />").val(this.ID_Section).text('(' + this.SectionID + ') ' + this.SectionName_EN));
                        });
                        $("#" + element.id).val(selectValue).trigger('change');
                    }
                }); 
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

function EditSection(id, code, name) {
    $("#txtSectionId").val(code);
    $("#txtSectionName").val(name); 
    $("#sectionId").val(id); 
}

function DeleteSection(id, code, name) {
    MessageController.ConfirmCallback("Are you sure you want to delete Section '" + code + ' ' + name + "'?", "Confirm Delete!", function (res) {
        if (res) { 
            $.ajax({
                type: "POST",
                url: '/Organization/DeleteSection',
                data: { 'sectionId': id },
                success: function (response) {
                    LoadSection();
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

function CreateSection(sectiomModel) { 
    $.ajax({
        type: "POST",
        url: '/Organization/CreateSection',
        data: sectiomModel,// { model: JSON.stringify(sectiomModel) },
        success: function (response) {
            $("#section-Editor-modal").magnificPopup('close');
            LoadSection(); 
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

function UpdateSection(sectiomModel) { 
    $.ajax({
        type: "POST",
        url: '/Organization/UpdateSection',
        data: sectiomModel,// { model: JSON.stringify(sectiomModel) },
        success: function (response) {
            $("#section-Editor-modal").magnificPopup('close');
            LoadSection();  
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

function CreateSectionPopup() {
    $('.section-Editor-modal').magnificPopup({
        type: 'inline',
        preloader: false,
        modal: true,
        callbacks: {
            beforeOpen: function () {
                var mp = this.st;
                if (mp) { 
                    var datatset = mp.el[0].dataset;
                    $("#txtSectionId").val(datatset.var1);
                    $("#txtSectionAbbreviation").val(datatset.var2); 
                    $("#txtSectionName").val(datatset.var3);                   
                } else {
                    $("#txtSectionId").val(null);
                    $("#txtSectionName").val(null);
                    $("#txtSectionAbbreviation").val(null); 
                } 
            },
            afterClose: function () {
                $("#txtSectionId").val(null);
                $("#txtSectionName").val(null);
                $("#txtSectionAbbreviation").val(null); 
            }
        }
    });

    $(document).on('click', '.popup-modal-dismiss', function (e) {
        e.preventDefault();
        $.magnificPopup.close();
    });
}

function CreatePopupImportFile() {
    $('.import-excel-modal').magnificPopup({
        type: 'inline',
        preloader: false,
        modal: true,
        callbacks: {
            beforeOpen: function () {
                if ($.fn.dataTable.isDataTable('#dtImportExcel')) {
                    var table = $('#dtImportExcel').DataTable();
                    table.destroy();
                }
                $('#dtImportExcel').DataTable({
                    'processing': true,
                    'paging': true,
                    "ordering": true,
                    "searching": false,
                    "lengthChange": false,
                    "bAutoWidth": false,
                    "Filter": false,
                    "info": false,
                    "bPaginate": false,
                    "pageLength": 10, 
                    "bLengthChange": false,
                    "bJQueryUI": true, //Enable smooth theme
                    "sPaginationType": "full_numbers", //Enable smooth theme
                    "sDom": 'lfrtip'
                });
                $('.dataTables_length').addClass('bs-select');
                $("#inputGroupFile01").val(null);
                $('#lblFileName').html("Choose file");
            },
            beforeClose: function () {
                $('#dtImportExcel').empty(); 
                $("#inputGroupFile01").val(null);
                $('#lblFileName').html("Choose file");
            }
        }
    });

    $(document).on('click', '.popup-modal-dismiss', function (e) {
        e.preventDefault();
        $.magnificPopup.close();
    });
} 

function uploadFiles() { 
    var input = document.getElementById("inputGroupFile01");
    var files = input.files;
    if (files.length <= 0) return;
    MessageController.BlockUI({ boxed: true, target: '#import-excel-modal' }); 
    var formData = new FormData(); 
    formData.append("file", files[0]); 
    formData.append('divisionId', CookiesController.getCookie("DIVISIONID"));
    formData.append('departmentId', CookiesController.getCookie("DEPARTMENTID"));
    //for (var i = 0; i != files.length; i++) {
    //    formData.append("files", files[i]); 
    $.ajax({
        url: '/Organization/ImportSection',
        type: 'post',
        data: formData,
        cache: false,
        contentType: false,
        processData: false,
        success: function (response) { 
            if ($.fn.dataTable.isDataTable('#dtImportExcel')) {
                var table = $('#dtImportExcel').DataTable();
                table.destroy();
            }
            $('#dtImportExcel').DataTable({
                'data': response.Data,
                'columns': [
                    { "data": 'EmployeeID', title: 'Employee ID', "className": 'text-center'}, 
                    { "data": 'FullName', title: 'Employee Name', "className": 'text-center' },
                    { "data": 'JobGrade', title: 'Job Grade', "className": 'text-center' },
                    { "data": 'JobTitle', title: 'Job Title', "className": 'text-center' },
                    { "data": 'Section', title: 'Section', "className": 'text-center'  }, 
                    {
                        "data": 'IsValid',
                        "sTitle": 'Is Valid',
                        "className": 'text-center',
                        "mRender": function (data, type, row) {
                            if (data == true) {
                                return '<img src="../assets/images/svg/icon-check-green.svg" class="light-logo" />'
                            } else {
                                return '<img src="../assets/images/svg/icon-cross-red.svg" class="light-logo" />'
                            }
                        }
                    },
                     { "data": 'Remark', title: 'Remark' }
                ],
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
                "bJQueryUI": true, //Enable smooth theme
                "sPaginationType": "full_numbers", //Enable smooth theme
                "sDom": 'lfrtip',
                "pageLength": 10,
                "language": {
                    "zeroRecords": "No data"
                }
            });
            $('.dataTables_length').addClass('bs-select');
            //var isfound = response.Data.any((item) => {
            //    return item.IsValid == false;
            //});
            //if (isfound == true) {
            //    $("#btnAllowImport").attr("disabled", "disabled");
            //} else {
            //    $("#btnAllowImport").removeAttr("disabled");
            //}
            MessageController.UnblockUI('#import-excel-modal');
        },
        failure: function (response) {
            MessageController.UnblockUI('#import-excel-modal');
            $("#inputGroupFile01").val(null);
            $('#lblFileName').html("Choose file");
            if (JSON.parse(response.responseText).Errors.length > 0) {
                MessageController.Error(JSON.parse(response.responseText).Errors[0].replace("Message:", ""), "Error");
            } else {
                MessageController.Error(response.responseText, "Error");
            }
        },
        error: function (response) {
            MessageController.UnblockUI('#import-excel-modal');
            $("#inputGroupFile01").val(null);
            $('#lblFileName').html("Choose file");
            if (JSON.parse(response.responseText).Errors.length > 0) {
                MessageController.Error(JSON.parse(response.responseText).Errors[0].replace("Message:", ""), "Error");
            } else {
                MessageController.Error(response.responseText, "Error");
            }
        }
    });
}