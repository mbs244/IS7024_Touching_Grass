// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

document.addEventListener('DOMContentLoaded', () => {
    const form = document.getElementById('watering-form');
    const submitButton = form?.querySelector('[data-submit-button]');
    const submitLabel = form?.querySelector('[data-submit-label]');
    const submitSpinner = form?.querySelector('[data-submit-spinner]');

    form?.addEventListener('submit', () => {
        if (!submitButton || !submitLabel || !submitSpinner) {
            return;
        }

        submitButton.disabled = true;
        submitLabel.textContent = 'Calculating…';
        submitSpinner.classList.remove('d-none');
        submitButton.setAttribute('aria-busy', 'true');
    });
});
