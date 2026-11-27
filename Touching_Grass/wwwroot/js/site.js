// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

document.addEventListener('DOMContentLoaded', () => {
    const form = document.getElementById('watering-form');
    const submitButton = form?.querySelector('[data-submit-button]');
    const submitLabel = form?.querySelector('[data-submit-label]');
    const submitSpinner = form?.querySelector('[data-submit-spinner]');
    const plantInput = form?.querySelector('[data-plant-input]');
    const selectedPlantId = form?.querySelector('[data-plant-id]');
    const suggestionsList = document.getElementById('plant-suggestions');
    const searchStatus = document.getElementById('plant-search-status');

    let autocompleteController;

    const hideSuggestions = () => {
        if (!suggestionsList) {
            return;
        }

        suggestionsList.innerHTML = '';
        suggestionsList.classList.add('d-none');
        plantInput?.setAttribute('aria-expanded', 'false');
    };

    const setSearchStatus = (message) => {
        if (!searchStatus) {
            return;
        }

        searchStatus.textContent = message;
        searchStatus.classList.toggle('d-none', !message);
    };

    const chooseSuggestion = (suggestion) => {
        if (!plantInput || !selectedPlantId) {
            return;
        }

        plantInput.value = suggestion.commonName;
        selectedPlantId.value = suggestion.plantId;
        hideSuggestions();
        setSearchStatus(`Selected: ${suggestion.displayLabel}`);
    };

    const renderSuggestions = (suggestions) => {
        if (!suggestionsList || !plantInput) {
            return;
        }

        suggestionsList.innerHTML = '';
        if (!suggestions.length) {
            hideSuggestions();
            return;
        }

        suggestions.forEach((suggestion) => {
            const item = document.createElement('li');
            item.className = 'list-group-item list-group-item-action plantwise-suggestion-item';
            item.setAttribute('role', 'option');
            item.textContent = suggestion.displayLabel;
            item.addEventListener('mousedown', (event) => {
                event.preventDefault();
                chooseSuggestion(suggestion);
            });
            suggestionsList.appendChild(item);
        });

        suggestionsList.classList.remove('d-none');
        plantInput.setAttribute('aria-expanded', 'true');
    };

    plantInput?.addEventListener('input', async () => {
        if (!selectedPlantId) {
            return;
        }

        const query = plantInput.value.trim();
        selectedPlantId.value = '';

        if (query.length < 3) {
            hideSuggestions();
            setSearchStatus('');
            return;
        }

        autocompleteController?.abort();
        autocompleteController = new AbortController();

        try {
            const response = await fetch(`/?handler=Autocomplete&query=${encodeURIComponent(query)}`, {
                signal: autocompleteController.signal,
                headers: {
                    Accept: 'application/json'
                }
            });

            if (!response.ok) {
                throw new Error(`Autocomplete request failed with status ${response.status}`);
            }

            const payload = await response.json();
            renderSuggestions(payload.suggestions || []);
            setSearchStatus(payload.noResultsMessage || '');
        } catch (error) {
            if (error.name === 'AbortError') {
                return;
            }

            hideSuggestions();
            setSearchStatus('Plant suggestions are temporarily unavailable.');
        }
    });

    document.addEventListener('click', (event) => {
        if (!form?.contains(event.target)) {
            hideSuggestions();
        }
    });

    form?.addEventListener('submit', () => {
        if (!submitButton || !submitLabel || !submitSpinner) {
            return;
        }

        const isValid = typeof window.jQuery === 'function'
            ? window.jQuery(form).valid()
            : form.checkValidity();

        if (!isValid) {
            return;
        }

        submitButton.disabled = true;
        submitLabel.textContent = 'Calculating…';
        submitSpinner.classList.remove('d-none');
        submitButton.setAttribute('aria-busy', 'true');
    });
});
