const apiUrl = (window.FORMS_API_URL || "").replace(/\/$/, "");
const tabs = document.querySelectorAll("[data-tab]");
const panels = {
  register: document.querySelector("#register-form"),
  check: document.querySelector("#check-form")
};
const feedback = document.querySelector("#feedback");

for (const tab of tabs) {
  tab.addEventListener("click", () => {
    const selected = tab.dataset.tab;
    for (const item of tabs) item.classList.toggle("is-active", item === tab);
    for (const [name, panel] of Object.entries(panels)) {
      const isSelected = name === selected;
      panel.hidden = !isSelected;
      panel.classList.toggle("is-visible", isSelected);
    }
    hideFeedback();
  });
}

panels.register.addEventListener("submit", async (event) => {
  event.preventDefault();
  await sendForm(panels.register, "/api/forms", "Cadastro realizado com sucesso.");
});

panels.check.addEventListener("submit", async (event) => {
  event.preventDefault();
  const result = await sendForm(panels.check, "/api/forms/check");
  if (result?.cadastrado) {
    showFeedback("Encontramos um cadastro com esses dados.", "success");
  } else if (result) {
    showFeedback("Não encontramos um cadastro com esses dados.", "info");
  }
});

async function sendForm(form, path, successMessage) {
  if (!form.reportValidity()) return null;

  const button = form.querySelector("button[type=submit]");
  const originalLabel = button.textContent;
  button.disabled = true;
  button.textContent = "Enviando...";
  hideFeedback();

  try {
    const response = await fetch(`${apiUrl}${path}`, {
      body: JSON.stringify(Object.fromEntries(new FormData(form))),
      headers: { "Content-Type": "application/json" },
      method: "POST"
    });
    const payload = await response.json().catch(() => ({}));
    if (!response.ok) throw new Error(payload.message || "Não foi possível concluir a operação.");
    if (successMessage) {
      form.reset();
      showFeedback(successMessage, "success");
    }
    return payload;
  } catch (error) {
    showFeedback(error.message || "Verifique se a API está disponível.", "error");
    return null;
  } finally {
    button.disabled = false;
    button.textContent = originalLabel;
  }
}

function showFeedback(message, type) {
  feedback.textContent = message;
  feedback.className = `feedback ${type}`;
  feedback.hidden = false;
}

function hideFeedback() {
  feedback.hidden = true;
  feedback.textContent = "";
  feedback.className = "feedback";
}
