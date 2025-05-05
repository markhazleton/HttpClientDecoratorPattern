// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(document).ready(function () {
    $('.table').DataTable();

    // Initialize color mode icon based on current theme
    updateColorModeUI();

    // Handle color mode toggle
    $('#colorModeToggle').on('click', function() {
        const currentMode = $('html').attr('data-bs-theme');
        const newMode = currentMode === 'dark' ? 'light' : 'dark';
        
        // Get current URL and parameters
        const url = new URL(window.location.href);
        const currentTheme = url.searchParams.get('theme');
        
        // Save the preference in a cookie
        document.cookie = `color-mode=${newMode};path=/;max-age=31536000`;
        
        // Update the HTML attribute
        $('html').attr('data-bs-theme', newMode);
        
        // Update UI elements
        updateColorModeUI();
        
        // If we're on a page with a specific theme parameter, preserve it when switching modes
        if (currentTheme) {
            // Refresh page to ensure the server-side rendering uses the updated cookie
            // but preserve the current theme selection
            window.location.href = window.location.href;
        }
    });

    // Function to update color mode UI elements
    function updateColorModeUI() {
        const currentMode = $('html').attr('data-bs-theme');
        
        if (currentMode === 'dark') {
            $('#colorModeIcon').removeClass('bi-sun-fill').addClass('bi-moon-fill');
            $('#colorModeText').text('Switch to Light Mode');
        } else {
            $('#colorModeIcon').removeClass('bi-moon-fill').addClass('bi-sun-fill');
            $('#colorModeText').text('Switch to Dark Mode');
        }
    }
});