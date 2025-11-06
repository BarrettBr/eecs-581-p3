const protocol = location.protocol === "https:" ? "wss" : "ws"; // Changed it to be dynamic based on how thery accessed it, this shouldn't be needed since we run http but nice for later
const host = location.host;
window.CONFIG = {
    socket_url: `${protocol}://${host}/ws`,
};
