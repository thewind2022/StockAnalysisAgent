"""
Stock Agent wrapper for LangSmith evaluation.
Calls the actual deployed AgentService API.
"""

import requests
import json
from typing import Dict, Any, List

class StockAgent:
    """
    A client that invokes the stock analysis agent via HTTP.
    Assumes the agent service is running at http://localhost:5001 (or override via env).
    """

    def __init__(self, base_url: str = "http://localhost:5001"):
        self.base_url = base_url.rstrip('/')
        self.memo_endpoint = f"{self.base_url}/api/agent/GenerateMemo"

    def invoke(self, input_data: Dict[str, Any]) -> Dict[str, Any]:
        """
        Invoke the agent with a given input.
        Expected input format: {"stockCode": "600036", "date": "2025-12-31"}
        """
        # Extract parameters from LangSmith input (usually contains "messages" or direct fields)
        # Adjust according to how your evaluation dataset is structured.
        if "messages" in input_data:
            # If using chat format, parse the last user message
            last_msg = input_data["messages"][-1]
            if last_msg["role"] == "user":
                content = last_msg["content"]
                # Simple parsing: expect "stockCode: xxx, date: yyyy-mm-dd"
                import re
                code_match = re.search(r"stockCode[\s:]*(\w+)", content, re.IGNORECASE)
                date_match = re.search(r"date[\s:]*(\d{4}-\d{2}-\d{2})", content, re.IGNORECASE)
                stock_code = code_match.group(1) if code_match else "600036"
                date_str = date_match.group(1) if date_match else "2025-12-31"
            else:
                stock_code = "600036"
                date_str = "2025-12-31"
        else:
            # Assume direct fields
            stock_code = input_data.get("stockCode", "600036")
            date_str = input_data.get("date", "2025-12-31")

        # Convert date string to datetime object or keep string as API expects
        payload = {
            "stockCode": stock_code,
            "date": date_str
        }

        try:
            response = requests.post(
                self.memo_endpoint,
                json=payload,
                headers={"Content-Type": "application/json"},
                timeout=30
            )
            response.raise_for_status()
            result = response.json()
            # Return a dict that LangSmith evaluators can understand
            return {
                "output": result,
                "stockCode": stock_code,
                "date": date_str
            }
        except requests.exceptions.RequestException as e:
            # Return error info so evaluators can mark as failure
            return {
                "output": None,
                "error": str(e),
                "stockCode": stock_code,
                "date": date_str
            }

# Optional: if you want to test standalone
if __name__ == "__main__":
    agent = StockAgent()
    test_input = {"stockCode": "600036", "date": "2025-12-31"}
    result = agent.invoke(test_input)
    print(json.dumps(result, indent=2))