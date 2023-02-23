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



