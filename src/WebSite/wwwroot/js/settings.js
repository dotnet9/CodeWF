class Settings
{
    constructor()
    {
        this.defaultTimeout = 1500;
    }

    taskCompleted(btn, timeout)
    {
        if (btn)
        {
            btn.classList.add("checked");
            setTimeout(() => btn.classList.remove("checked"), timeout ?? this.defaultTimeout);
        }
    }

    openBtns(containerid)
    {
        const container = document.getElementById(containerid);
        container.classList.toggle("open");
    }

    goBack()
    {
        window.history.back();
    }

    goToTop()
    {
        window.scrollTo({
            top: 0,
            left: 0,
            behavior: "smooth"
        });
    }

    goToBottom()
    {
        window.scrollTo({
            top: document.body.scrollHeight,
            left: 0,
            behavior: "smooth"
        });
    }
}