function handleAPIErrors(response) {
    if (!response.ok) {
        throw response;
    }
    return response;
}

export default { handleAPIErrors }
