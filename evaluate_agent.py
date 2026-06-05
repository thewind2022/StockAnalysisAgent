import pytest
from langsmith import Client
from langsmith.evaluation import evaluate
from my_agent import StockAgent

client = Client()

def evaluate_agent():
    # 定义评估任务
    evaluation_results = evaluate(
        lambda input: StockAgent().invoke(input["messages"]),
        data=client.list_examples(dataset_name="Stock Query Dataset"),
        evaluators=[
            semantic_similarity_evaluator(),
            correct_stock_code_evaluator(),
            is_valid_json_evaluator()
        ],
        experiment_prefix="stock-agent-v1",
        metadata={"version": "1.0.0"}
    )
    print("评估完成，结果:", evaluation_results)

if __name__ == "__main__":
    evaluate_agent()