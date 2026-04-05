module.exports = async function (context, req) {
    context.log('JavaScript HTTP trigger function processed a request.');

    const name = (req.query.name || (req.body && req.body.name));
    const currentTime = new Date().toISOString();
    const responseMessage = name
        ? "Hello, " + name + ". This HTTP triggered function executed successfully. Current time: " + currentTime
        : "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response. Current time: " + currentTime;


    context.res = {
        // status: 200, /* Defaults to 200 */
        body: responseMessage
    };
}