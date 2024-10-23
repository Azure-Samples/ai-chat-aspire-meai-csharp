export function submitOnEnter(formElem) {
    formElem.addEventListener('keydown', e => {
        if (e.key === 'Enter' && !e.ctrlKey && !e.shiftKey && !e.altKey && !e.metaKey) {
            e.srcElement.dispatchEvent(new Event('change', { bubbles: true }));
            formElem.requestSubmit();
        }
    });

    formElem.addEventListener('submit', e => {
        // Scroll the last message into view
        var lastMessage = document.querySelector(".messages-scroller").lastChild;
        lastMessage.scrollIntoView({behavior: "instant", block: "end"});
    });
}