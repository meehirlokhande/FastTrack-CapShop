import { cn } from "../../utils/cn";

export default function Stepper({ steps, currentStep }) {
  return (
    <div className="flex items-center justify-center mb-8">
      {steps.map((step, idx) => {
        const stepNum = idx + 1;
        const isDone = stepNum < currentStep;
        const isActive = stepNum === currentStep;

        return (
          <div key={step} className="flex items-center">
            <div className="flex flex-col items-center">
              <div
                className={cn(
                  "w-9 h-9 rounded-full flex items-center justify-center text-sm font-semibold border-2 transition-colors",
                  isDone
                    ? "bg-indigo-600 border-indigo-600 text-white"
                    : isActive
                    ? "border-indigo-600 text-indigo-600 bg-white"
                    : "border-gray-300 text-gray-400 bg-white"
                )}
              >
                {isDone ? (
                  <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                  </svg>
                ) : (
                  stepNum
                )}
              </div>
              <span
                className={cn(
                  "mt-1 text-xs font-medium",
                  isActive ? "text-indigo-600" : isDone ? "text-indigo-400" : "text-gray-400"
                )}
              >
                {step}
              </span>
            </div>
            {idx < steps.length - 1 && (
              <div
                className={cn(
                  "h-0.5 w-16 sm:w-24 mx-2 mb-4 transition-colors",
                  stepNum < currentStep ? "bg-indigo-600" : "bg-gray-200"
                )}
              />
            )}
          </div>
        );
      })}
    </div>
  );
}
