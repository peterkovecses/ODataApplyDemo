using ODataApplyDemo.Models;

namespace ODataApplyDemo.Exceptions;

public class ODataApplyException(OdataResponseWrapper response) : Exception
{
    public OdataResponseWrapper Response => response;
}
