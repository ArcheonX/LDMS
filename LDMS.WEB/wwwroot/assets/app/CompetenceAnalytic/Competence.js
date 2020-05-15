(function ($) {
    "use strict";
    $(document).ready(function () {
        $('#btnAddCompetence').click(function () {
            window.location.href = "Add"; 
        });
        Search();
    })
})(jQuery); 

function Search() {
    MessageController.BlockUI({ boxed: true, target: '#dtCompetenceList' });
    if ($.fn.dataTable.isDataTable('#dtCompetenceList')) {
        var table = $('#dtCompetenceList').DataTable();
        table.destroy();
    }
    $.ajax({
        type: "GET",
        url: "/Competence/Analytics", 
        success: function (response) {
            $('#dtCompetenceList').DataTable({
                'data': response.Data,
                'columns': [
                    { data: 'RowIndex', title: '#', "className": "text-center"}, 
                    { data: 'CompetenceAnalyticName', title: 'Competence Platform' },
                    { data: 'JobGrade', title: 'Level', "className": "text-center" },
                    {
                        "mData": "LastUpdate",
                        "bSortable": true,
                        "title": 'Last Update',
                        "className": "text-center",
                        "mRender": function (data, type, row) {
                            return moment(data).format("DD/MM/YYYY HH:mm:ss");
                        }
                    }, 
                    {
                        "mData": "ID_Analytic",
                        "className": "text-center",
                        "mRender": function (data, type, row) {
                            var url = '../Competence/Edit/' + data; 
                            return "<a href='" + url + "'><img src='../assets/images/svg/icon-edit-blue.svg' class='light-logo' alt='homepage'  style='width:30px;height:30px;' /></a>";
                        }
                    },
                    {
                        "mData": "ID_Analytic",
                        "className": "text-center",
                        "mRender": function (data, type, row) {
                            return "<a href='#' onclick='OnDelete(" + data + ")'><img src='../assets/images/svg/icon-delete-red.svg' class='light-logo' alt='homepage'  style='width:30px;height:30px;' /></a>";
                        }
                    },
                    {
                        "mData": "ID_Analytic",
                        "className": "text-center",
                        "mRender": function (data, type, row) {
                            var url = '../Competence/Analytic/' + data; 
                            return "<a href='" + url + "'><img src='../assets/images/svg/pie-chart.svg' class='light-logo' alt='homepage' style='width:30px;height:30px;' /></a>";
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
                "bJQueryUI": true, //Enable smooth theme
                "sPaginationType": "full_numbers", //Enable smooth theme
                "sDom": 'lfrtip',
                "pageLength": 20,
                "language": {
                    "zeroRecords": "No Employee"
                }
            });
            $('.dataTables_length').addClass('bs-select');
            MessageController.UnblockUI('#dtCompetenceList');
        }
    }); 
}

function OnDelete(Id) {
    MessageController.WarningCallback("Are you sure you want to delete Competence Platform?", "Confirm Delete!", function (res) {
        if (res) {
            $.ajax({
                type: "Delete",
                url: '/Competence/Delete',
                data: { 'competenceId': Id },
                success: function (response) {
                    Search();
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