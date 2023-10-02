// SimpleServer based on code by Can Güney Aksakalli
// MIT License - Copyright (c) 2016 Can Güney Aksakalli
// https://aksakalli.github.io/2014/02/24/simple-http-server-with-csparp.html
// modifications by Jaime Spacco

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Web;
using System.Text.Json;


/// <summary>
/// Interface for simple servlets.
/// 
/// </summary>
interface IServlet {
    void ProcessRequest(HttpListenerContext context);
}
/// <summary>
/// BookHandler: Servlet that reads a JSON file and returns a random book
/// as an HTML table with one row.
/// TODO: search for specific books by author or title or whatever
/// </summary>
class BookHandler : IServlet {

    private List<Book> books;
    

    public BookHandler()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        string text = File.ReadAllText(@"json/books.json");
        books = JsonSerializer.Deserialize<List<Book>>(text, options);
    }

    public void ProcessRequest(HttpListenerContext context) {
        string response = "";
        if(context.Request.QueryString.AllKeys.Contains("cmd")){
            string cmd = context.Request.QueryString["cmd"];
            if(cmd.Equals("list")){
                int start = Int32.Parse(context.Request.QueryString["s"]);
                int end = Int32.Parse(context.Request.QueryString["e"]);
                List<Book> sublist = books.GetRange(start, end - start + 1);

                response = $@"
            <table border=1>
            <tr>
                <th>Title</th>
                <th>Author</th>
                <th>Short Description</th>
                <th>Long Description</th>
            </tr>";
            foreach (Book book in sublist)
            {
                string authors = String.Join(",<br>", book.Authors);
                response += $@"
            <tr>
                <td>{book.Title}</td>
                <td>{authors}</td>
                <td>{book.ShortDescription}</td>
                <td>{book.LongDescription}</td>
            </tr>";
            }
            response += "</table>";
            }
        }else if(context.Request.QueryString.AllKeys.Contains("n")){
            int bookNum = Int32.Parse(context.Request.QueryString["n"]);
            Book book = books[bookNum];

            // convert book.Authors, which is a list, into a string with ", <br>" in between each author
            // string.Join() is a very useful method
            string delimiter = ",<br> ";
            string authors = string.Join(delimiter, book.Authors);

            // build the HTML response
            // @ means a multiline string (Java doesn't have this)
            // $ means string interpolation (Java doesn't have this either)
            response = $@"
            <table border=1>
            <tr>
                <th>Title</th>
                <th>Author</th>
                <th>Short Description</th>
                <th>Long Description</th>
            </tr>
            <tr>
                <td>{book.Title}</td>
                <td>{authors}</td>
                <td>{book.ShortDescription}</td>
                <td>{book.LongDescription}</td>
            </tr>
            </table>
            ";
        }else{
            Book book = books[4];

            // convert book.Authors, which is a list, into a string with ", <br>" in between each author
            // string.Join() is a very useful method
            string delimiter = ",<br> ";
            string authors = string.Join(delimiter, book.Authors);

            // build the HTML response
            // @ means a multiline string (Java doesn't have this)
            // $ means string interpolation (Java doesn't have this either)
            response = $@"
            <table border=1>
            <tr>
                <th>Title</th>
                <th>Author</th>
                <th>Short Description</th>
                <th>Long Description</th>
            </tr>
            <tr>
                <td>{book.Title}</td>
                <td>{authors}</td>
                <td>{book.ShortDescription}</td>
                <td>{book.LongDescription}</td>
            </tr>
            </table>
            ";
        }

        SimpleHTTPServer.Respond(response, context);

    }
}
/// <summary>
/// FooHandler: Servlet that returns a simple HTML page.
/// </summary>
class FooHandler : IServlet {

    public void ProcessRequest(HttpListenerContext context) {
        string response = $@"
            <H1>This is a Servlet Test.</H1>
            <h2>Servlets are a Java thing; there is probably a .NET equivlanet but I don't know it</h2>
            <h3>I am but a humble Java programmer who wrote some Servlets in the 2000s</h3>
            <p>Request path: {context.Request.Url.AbsolutePath}</p>
";
        foreach ( String s in context.Request.QueryString.AllKeys )
            response += $"<p>{s} -> {context.Request.QueryString[s]}</p>\n";

        SimpleHTTPServer.Respond(response, context);
    }
}

class ErrorHandler : IServlet {
    public void ProcessRequest(HttpListenerContext context) {
    string response = $@"
            <H1>Custom 404: Page Not Found.</H1>
            <h2>Check the URL again</h2>
            <p>Request path: {context.Request.Url.AbsolutePath}</p>
";
        SimpleHTTPServer.Respond(response, context);
    }
}

class Book2Handler : IServlet {
    private List<Book> books;

    public Book2Handler()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        string text = File.ReadAllText(@"json/books.json");
        books = JsonSerializer.Deserialize<List<Book>>(text, options);
    }

    public void ProcessRequest(HttpListenerContext context) {
        string response = "";
        if(context.Request.QueryString.AllKeys.Contains("a")){
            string author = context.Request.QueryString["a"];
            IEnumerable<Book> searchResult = books.Where(n => String.Join(",<br>", n.Authors).ToLower().Contains(author.ToLower()));

            response = $@"
            <table border=1>
            <tr>
                <th>Title</th>
                <th>Author</th>
                <th>Short Description</th>
                <th>Long Description</th>
            </tr>";
            foreach (Book book in searchResult)
            {
                string authors = String.Join(",<br>", book.Authors);
                response += $@"
            <tr>
                <td>{book.Title}</td>
                <td>{authors}</td>
                <td>{book.ShortDescription}</td>
                <td>{book.LongDescription}</td>
            </tr>";
            }

            SimpleHTTPServer.Respond(response, context);
        }
    }
}

/// <summary>
/// ShowHandler: Servlet that reads a JSON file and returns shows.
/// Shows can be looked up by either the name or the season number
/// </summary>
class ShowHandler : IServlet {
    private List<Show> shows;

    public ShowHandler()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        string text = File.ReadAllText(@"json/buffy.json");
        shows = JsonSerializer.Deserialize<List<Show>>(text, options);
    }
    public void ProcessRequest(HttpListenerContext context) {
        string response = "";
        if(context.Request.QueryString.AllKeys.Contains("n")){
            string name = context.Request.QueryString["n"];
            IEnumerable<Show> searchResult = shows.Where(n => n.Name.ToLower().Contains(name.ToLower()));

            response = $@"
            <table border=1>
            <tr>
                <th>Title</th>
                <th>Summary</th>
                <th>Runtime</th>
                <th>URL</th>
            </tr>";
            foreach (Show show in searchResult)
            {
                response += $@"
            <tr>
                <td>{show.Name}</td>
                <td>{show.Summary}</td>
                <td>{show.Runtime}</td>
                <td>{show.Url}</td>
            </tr>";
            }

            SimpleHTTPServer.Respond(response, context);
        }else if(context.Request.QueryString.AllKeys.Contains("s")){
            int season = Int32.Parse(context.Request.QueryString["s"]);
            IEnumerable<Show> searchResult = shows.Where(n => n.Season == season);

            response = $@"
            <table border=1>
            <tr>
                <th>Title</th>
                <th>Summary</th>
                <th>Season</th>
                <th>Number</th>
                <th>URL</th>
            </tr>";
            foreach (Show show in searchResult)
            {
                response += $@"
            <tr>
                <td>{show.Name}</td>
                <td>{show.Summary}</td>
                <td>{show.Season}</td>
                <td>{show.Number}</td>
                <td>{show.Url}</td>
            </tr>";
            }

            SimpleHTTPServer.Respond(response, context);
        }else{
            response = $@"
            <table border=1>
            <tr>
                <th>Title</th>
                <th>Summary</th>
                <th>Runtime</th>
                <th>URL</th>
            </tr>";
            SimpleHTTPServer.Respond(response, context);
        }
    }
}

class SimpleHTTPServer
{
    // bind servlets to a path
    // for example, this means that /foo will be handled by an instance of FooHandler
    // TODO: put these mappings into a configuration file
    private static IDictionary<string, IServlet> _servlets = new Dictionary<string, IServlet>() {
        {"foo", new FooHandler()},
        {"books", new BookHandler()},
        {"error", new ErrorHandler()},
        {"books2", new Book2Handler()},
        {"shows", new ShowHandler()}
    };

    // list of default index files
    // if the client requests a directory (e.g. http://localhost:8080/), 
    // we will look for one of these files
    private string[] _indexFiles;
    
    // map extensions to MIME types
    // TODO: put this into a configuration file
    private IDictionary<string, string> _mimeTypeMappings;

    // instance variables
    private Thread _serverThread;
    private string _rootDirectory;
    private HttpListener _listener;
    private int _numRequests = 0;
    private int _port;
    private bool _done = false;
    private Dictionary<string, int> pathsRequested = new Dictionary<string, int>();

    private Dictionary<string, int> errorPathsRequested = new Dictionary<string, int>();

    public int Port
    {
        get { return _port; }
        private set { }
    }

    public int NumRequests
    {
        get { return _numRequests; }
        private set { _numRequests = value; }
    }

    public Dictionary<string, int> ErrorPathsRequested
    {
        get{ return errorPathsRequested; }
    }

    public Dictionary<string, int> PathsRequested
    {
        get{ return pathsRequested; }
    }

    /// <summary>
    /// Construct server with given port.
    /// </summary>
    /// <param name="path">Directory path to serve.</param>
    /// <param name="port">Port of the server.</param>
    public SimpleHTTPServer(string path, int port, string configFilename)
    {
        this.Initialize(path, port, configFilename);
    }

    /// <summary>
    /// Construct server with any open port.
    /// </summary>
    /// <param name="path">Directory path to serve.</param>
    public SimpleHTTPServer(string path, string configFilename)
    {
        //get an empty port
        TcpListener l = new TcpListener(IPAddress.Loopback, 0);
        l.Start();
        int port = ((IPEndPoint)l.LocalEndpoint).Port;
        l.Stop();
        this.Initialize(path, port, configFilename);
    }

    /// <summary>
    /// Stop server and dispose all functions.
    /// </summary>
    public void Stop()
    {
        _done = true;
        _listener.Close();
    }

    public static void Respond(string response, HttpListenerContext context){
        // write HTTP response to the output stream
        // all of the context.response stuff is setting the headers for the HTTP response
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(response);
        context.Response.ContentType = "text/html";
        context.Response.ContentLength64 = bytes.Length;
        context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
        context.Response.AddHeader("Last-Modified", DateTime.Now.ToString("r"));
        context.Response.StatusCode = (int)HttpStatusCode.OK;
        context.Response.OutputStream.Write(bytes, 0, bytes.Length);
        context.Response.OutputStream.Flush();
        context.Response.OutputStream.Close();
    }

    private void Listen()
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add("http://*:" + _port.ToString() + "/");
        _listener.Start();
        while (!_done)
        {
            Console.WriteLine("Waiting for connection...");
            try
            {
                HttpListenerContext context = _listener.GetContext();
                NumRequests += 1;
                Process(context);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        Console.WriteLine("Server stopped!");
    }

    /// <summary>
    /// Process an incoming HTTP request with the given context.
    /// </summary>
    /// <param name="context"></param>
    private void Process(HttpListenerContext context)
    {
        string filename = context.Request.Url.AbsolutePath;
        if(filename.Equals("/foo") || filename.Equals("/books") || filename.Equals("/") || filename.Equals("/error") || filename.Equals("/books2") || filename.Equals("/shows")){
            pathsRequested[filename] = pathsRequested.GetValueOrDefault(filename, 0) + 1;
        }else{
            errorPathsRequested[filename] = errorPathsRequested.GetValueOrDefault(filename, 0) + 1;
        }
        filename = filename.Substring(1);
        Console.WriteLine($"{filename} is the path");

        // check if the path is mapped to a servlet
        if (_servlets.ContainsKey(filename))
        {
            Console.WriteLine(filename);
            _servlets[filename].ProcessRequest(context);
            return;
        }

        // if the path is empty (i.e. http://blah:8080/ which yields hte path /)
        // look for a default index filename
        if (string.IsNullOrEmpty(filename))
        {
            foreach (string indexFile in _indexFiles)
            {
                if (File.Exists(Path.Combine(_rootDirectory, indexFile)))
                {
                    filename = indexFile;
                    break;
                }
            }
        }

        // search for the file in the root directory
        // this means we are serving the file, if we can find it
        filename = Path.Combine(_rootDirectory, filename);

        if (File.Exists(filename))
        {
            try
            {
                Console.WriteLine(filename);
                Stream input = new FileStream(filename, FileMode.Open);
                
                //Adding permanent http response headers
                string mime;
                context.Response.ContentType = _mimeTypeMappings.TryGetValue(Path.GetExtension(filename), out mime) ? mime : "application/octet-stream";
                context.Response.ContentLength64 = input.Length;
                context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
                context.Response.AddHeader("Last-Modified", System.IO.File.GetLastWriteTime(filename).ToString("r"));

                byte[] buffer = new byte[1024 * 16];
                int nbytes;
                while ((nbytes = input.Read(buffer, 0, buffer.Length)) > 0)
                    context.Response.OutputStream.Write(buffer, 0, nbytes);
                input.Close();
                
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.OutputStream.Flush();
                context.Response.OutputStream.Close();
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }

        }
        else
        {
            // This sends a 404 if the file doesn't exist or cannot be read
            // TODO: customize the 404 page

            // context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            _servlets["error"].ProcessRequest(context);
            return;
        }
        
        context.Response.OutputStream.Close();
    }

    /// <summary>
    /// Initializes the server by setting up a listener thread on the given port
    /// </summary>
    /// <param name="path">the path of the root directory to serve files</param>
    /// <param name="port">the port to listen for connections</param>
    private void Initialize(string path, int port, string configFilename)
    {
        this._rootDirectory = path;
        this._port = port;

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        string text = File.ReadAllText(configFilename);
        var config = JsonSerializer.Deserialize<Config>(text, options);
        _mimeTypeMappings = config.MimeTypes;
        _indexFiles = config.IndexFiles.ToArray();

        _serverThread = new Thread(this.Listen);
        _serverThread.Start();
    }


}
