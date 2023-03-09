// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(() => {

})


var toggleVisible = (id) =>
{
    // console.log(typeof(id))
    // console.log(id)
    let passwordInput = document.getElementById(id);
    let openEye = document.getElementById("open"+id)
    let closeEye = document.getElementById("close"+id)

    if (passwordInput.type === "password") {
        passwordInput.type = "text";
        openEye.style.display = "inline"
        closeEye.style.display = "none"
    } else {
        passwordInput.type = "password";
        openEye.style.display = "none"
        closeEye.style.display = "inline"
    }
}

var confirmDelete = (uniqueId, isDeleteClicked) => {
    var deleteSpan = 'delete_' + uniqueId;
    var confirmDeleteSpan = 'confirmDelete_' + uniqueId;

    if (isDeleteClicked) {
        $('#' + deleteSpan).hide();
        $('#' + confirmDeleteSpan).show();
    } else {
        $('#' + deleteSpan).show();
        $('#' + confirmDeleteSpan).hide();
    }
}

var autosave = () =>
{
    // Get the form data
    var formData = $('#edituserInfoForm').serializeArray();

    // console.log(formData);

    if (formData != null && formData != undefined && formData.length > 0)
    {
        let sendToServer = {
            UserName: formData.find(o => o.name === 'UserName')['value'],
            Email: formData.find(o => o.name === 'Email')['value'],
            FirstName: formData.find(o => o.name === 'FirstName')['value'],
            LastName: formData.find(o => o.name === 'LastName')['value'],
            Role: formData.find(o => o.name === 'Role')['value']
        }

        // console.log(sendToServer)

        // Send an AJAX request to the autosave action
        $.ajax({
            type: 'POST',
            url: '/Settings/Autosave',
            data: {autoSaveModel: JSON.stringify(sendToServer)},
            success: function(result, status, xhr) {
                console.log(`Autosave successful with status code: ${xhr.status}`);
            },
            error: function(xhr, status, error) {
                console.log('Autosave failed: ' + error + `status code: ${xhr.status}`);
            }
        });
    }


}
